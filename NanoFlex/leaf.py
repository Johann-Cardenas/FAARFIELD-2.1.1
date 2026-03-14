"""
LEAF — Layered Elastic Analysis Foundation solver for NanoFlex.

Faithful port of LEAFClassLib/clsLEAF.vb.  Uses Burmister-type layered elastic
theory with Gauss–Laguerre quadrature over the Hankel transform variable α.

Internal arrays use 1-based indexing (element 0 is padding) to maintain exact
correspondence with the VB.NET / Fortran-origin source.
"""

from __future__ import annotations

import math
from dataclasses import dataclass, field
from enum import IntEnum
from typing import Optional

import numpy as np
from numpy.typing import NDArray

from numerical import (bessj0, bessj1, bessj1_over_x,
                       _bessj0_vec, _bessj1_over_x_vec,
                       gaulag, gauss_jordan, lu_solve)

# ═══════════════════════════════════════════════════════════════════════════════
#  Constants
# ═══════════════════════════════════════════════════════════════════════════════

PI = 3.14159265359
NOFF = 41                # lateral offsets for CDF
OFFSET_INC = 10.0        # inches between offsets
N_NODES_LONG = 1800      # longitudinal nodes for tandem CDF

# ═══════════════════════════════════════════════════════════════════════════════
#  Data structures
# ═══════════════════════════════════════════════════════════════════════════════

class ResponseType(IntEnum):
    VERTICAL_STRAIN = 1
    VERTICAL_DEFLECTION = 2
    HORIZONTAL_STRESS = 3
    ALL_RESPONSES = 4


class SolverType(IntEnum):
    NONE = 0
    PART_INVERT = 1
    GAUSS_JORDAN = 2
    LU = 3
    GAUSS = 4


@dataclass
class LEAFAircraft:
    """Aircraft gear parameters (mirrors LEAFACParms)."""
    name: str
    gear_load: float               # total gear load (lbs)
    n_tires: int
    tire_press: NDArray             # 1-based [n_tires+1]
    tire_x: NDArray                 # 1-based [n_tires+1]
    tire_y: NDArray                 # 1-based [n_tires+1]
    n_eval_points: int
    eval_x: NDArray                 # 1-based [n_eval_points+1]
    eval_y: NDArray                 # 1-based [n_eval_points+1]
    gear: str = ""
    gear_orientation: int = 0


@dataclass
class LEAFStructure:
    """Pavement structure parameters (mirrors LEAFStrParms)."""
    n_layers: int
    thick: NDArray                  # 1-based [n_layers+1]
    modulus: NDArray                # 1-based [n_layers+1]
    poisson: NDArray                # 1-based [n_layers+1]
    interface_parm: NDArray         # 1-based [n_layers+1]
    eval_depth: float               # depth of evaluation point (in)
    eval_layer: int                 # layer index for evaluation


# ═══════════════════════════════════════════════════════════════════════════════
#  LEAF Solver
# ═══════════════════════════════════════════════════════════════════════════════

class LEAFSolver:
    """Layered elastic analysis solver.

    Computes stresses, strains, and deflections in a multi-layered elastic
    half-space under circular surface loads via Gauss–Laguerre quadrature
    of the Hankel-transform integrals.
    """

    def __init__(self, n_gauss: int = 500):
        gl_x, gl_w, self._gl_n = gaulag(n_gauss, 0.0)
        # Convert to 1-based: index 0 is padding
        self._gl_alpha = np.concatenate([[0.0], gl_x])
        self._gl_weight = np.concatenate([[0.0], gl_w])
        self._convergence_limit = 1e-6
        self._design_type: Optional[str] = None

    # ── Public API ────────────────────────────────────────────────────────

    def compute_response(
        self,
        response_type: ResponseType,
        aircraft_list: list[LEAFAircraft],
        structure: LEAFStructure,
    ) -> NDArray[np.float64]:
        """Compute elastic response for all aircraft at evaluation points.

        Returns response[iac][ieval] (1-based indices).
        """
        self._setup(aircraft_list, structure)

        if response_type == ResponseType.VERTICAL_STRAIN:
            return self._integrate_z_strain(self._eval_layer, self._z_eval)
        elif response_type == ResponseType.HORIZONTAL_STRESS:
            return self._integrate_h_stress(self._eval_layer, self._z_eval)
        elif response_type == ResponseType.VERTICAL_DEFLECTION:
            return self._integrate_z_deflection(self._eval_layer, self._z_eval)
        else:
            raise NotImplementedError("AllResponses not yet implemented")

    # ── Setup ─────────────────────────────────────────────────────────────

    def _setup(self, ac_list: list[LEAFAircraft], struct: LEAFStructure):
        """Prepare internal arrays from user-facing data structures."""
        self._detect_design_type(struct)

        # Dummy top layer for numerical stability (always enabled)
        n_orig = struct.n_layers
        if n_orig > 1 and struct.thick[1] < 2:
            raise ValueError("Top layer must be >= 2 inches thick")

        nl = n_orig + 1  # with dummy top
        self._n_layers = nl

        # 1-based arrays
        self._h_layer = np.zeros(nl + 1)
        self._youngs = np.zeros(nl + 1)
        self._poissons = np.zeros(nl + 1)
        self._interface_parm = np.zeros(nl + 1)

        # Dummy top: 1 inch of the same material as layer 1
        self._h_layer[1] = 1.0
        self._youngs[1] = struct.modulus[1]
        self._poissons[1] = struct.poisson[1]
        self._interface_parm[1] = 1.0  # fully bonded to itself

        for i in range(2, nl + 1):
            if i == 2:
                self._h_layer[i] = struct.thick[i - 1] - 1.0  # subtract dummy
            else:
                self._h_layer[i] = struct.thick[i - 1]
            self._youngs[i] = struct.modulus[i - 1]
            self._poissons[i] = struct.poisson[i - 1]
            self._interface_parm[i] = struct.interface_parm[i - 1]

        self._h_layer[0] = 0.0
        self._h_layer[nl] = self._h_layer[nl - 1]  # semi-infinite subgrade

        self._eval_layer = int(struct.eval_layer) + 1  # shift for dummy top
        self._eval_depth = struct.eval_depth

        # Interface depths
        self._z_interface = np.zeros(nl + 1)
        for i in range(1, nl):
            self._z_interface[i] = self._z_interface[i - 1] + self._h_layer[i]

        self._z_eval = self._eval_depth

        # Aircraft data
        nac = len(ac_list)
        self._nac = nac
        n_tires_max = max(ac.n_tires for ac in ac_list)
        n_eval_max = max(ac.n_eval_points for ac in ac_list)
        self._n_tires_max = n_tires_max
        self._n_eval_max = n_eval_max

        self._n_tires = np.zeros(nac + 1, dtype=int)
        self._n_eval_pts = np.zeros(nac + 1, dtype=int)
        self._gear_load = np.zeros(nac + 1)
        self._tire_press = np.zeros((nac + 1, n_tires_max + 1))
        self._tire_radius = np.zeros((nac + 1, n_tires_max + 1))
        self._tire_x = np.zeros((nac + 1, n_tires_max + 1))
        self._tire_y = np.zeros((nac + 1, n_tires_max + 1))
        self._eval_x = np.zeros((nac + 1, n_eval_max + 1))
        self._eval_y = np.zeros((nac + 1, n_eval_max + 1))
        self._radius = np.zeros((nac + 1, n_tires_max + 1, n_eval_max + 1))

        for idx, ac in enumerate(ac_list):
            iac = idx + 1
            self._n_tires[iac] = ac.n_tires
            self._n_eval_pts[iac] = ac.n_eval_points
            self._gear_load[iac] = ac.gear_load

            wheel_load = ac.gear_load / ac.n_tires
            for it in range(1, ac.n_tires + 1):
                self._tire_press[iac, it] = ac.tire_press[it]
                self._tire_radius[iac, it] = math.sqrt(
                    wheel_load / (ac.tire_press[it] * PI)
                )
                self._tire_x[iac, it] = ac.tire_x[it]
                self._tire_y[iac, it] = ac.tire_y[it]

            for ie in range(1, ac.n_eval_points + 1):
                self._eval_x[iac, ie] = ac.eval_x[ie]
                self._eval_y[iac, ie] = ac.eval_y[ie]

            for it in range(1, ac.n_tires + 1):
                for ie in range(1, ac.n_eval_points + 1):
                    dx = self._eval_x[iac, ie] - self._tire_x[iac, it]
                    dy = self._eval_y[iac, ie] - self._tire_y[iac, it]
                    self._radius[iac, it, ie] = math.sqrt(dx * dx + dy * dy)

        # Solver state
        self._override_solver = SolverType.NONE
        self._leaf_solver = SolverType.NONE
        self._pi_failed = [False, False, False]   # per call-number (1, 2)
        self._lu_failed = [False, False, False]

    def _detect_design_type(self, s: LEAFStructure):
        ip = s.interface_parm
        if s.n_layers < 2:
            self._design_type = None
            return
        if ip[1] == 1 and ip[2] == 0:
            self._design_type = "FlexOnRigid"
        elif ip[1] == 0 and ip[2] == 1:
            self._design_type = "NewRigid"
        elif ip[1] == 0 and ip[2] == 0:
            self._design_type = "UnbondOnRigid"
        elif 0 < ip[1] < 1:
            self._design_type = "PartBondOnRigid"
        elif ip[1] == 1 and ip[2] == 1 and s.modulus[2] == 200000:
            self._design_type = "FlexOnFlex"
        else:
            self._design_type = None

    # ── Origin shifts ─────────────────────────────────────────────────────

    def _set_o_shifts(self, il: int, z_layer: float, r_max: float) -> NDArray:
        """Compute origin shifts to stabilise exponentials in the integrand."""
        nl = self._n_layers
        o_shift = np.zeros((nl + 1, 3))  # indices [layer][1..2]
        z_min = 0.75 * r_max / 3.0

        if il != 1:
            o_shift[1, 1] = 2.0 * self._h_layer[1]
            o_shift[1, 2] = 2.0 * self._h_layer[1]
        else:
            o_shift[1, 1] = 2.0
            o_shift[1, 2] = 2.0

        for i in range(2, nl + 1):
            o_shift[i, 1] = o_shift[i - 1, 2] + self._h_layer[i - 1]
            o_shift[i, 2] = o_shift[i, 1]

        if o_shift[il, 1] < z_min + z_layer:
            o_shift[il, 1] = z_min + z_layer
            o_shift[il, 2] = z_min + z_layer
            for i in range(il - 1, 0, -1):
                o_shift[i, 1] = o_shift[i + 1, 2] - self._h_layer[i]
                o_shift[i, 2] = o_shift[i, 1]
            for i in range(il + 1, nl + 1):
                o_shift[i, 1] = o_shift[i - 1, 2] + self._h_layer[i - 1]
                o_shift[i, 2] = o_shift[i, 1]

        return o_shift

    def _get_max_parms(self):
        """Get maximum eval points, tires, and radius across all aircraft."""
        nac = self._nac
        n_eval_max = 0
        n_tires_max = 0
        r_max = 0.0
        a_max = 0.0
        for iac in range(1, nac + 1):
            if self._n_eval_pts[iac] > n_eval_max:
                n_eval_max = self._n_eval_pts[iac]
            if self._n_tires[iac] > n_tires_max:
                n_tires_max = self._n_tires[iac]
            if self._tire_radius[iac, 1] > a_max:
                a_max = self._tire_radius[iac, 1]
            for it in range(1, self._n_tires[iac] + 1):
                for ie in range(1, self._n_eval_pts[iac] + 1):
                    r2 = self._radius[iac, it, ie]
                    if r2 > r_max:
                        r_max = r2
        if a_max > r_max:
            r_max = a_max
        return n_eval_max, n_tires_max, r_max

    # ── Load function ─────────────────────────────────────────────────────

    @staticmethod
    def _load_function(gl_alpha: float, a: float) -> float:
        """Uniform circular load kernel: J₁(α·a)."""
        return bessj1(gl_alpha * a)

    # ── FindConstants dispatcher ──────────────────────────────────────────

    def _find_constants(
        self, alpha_by_z: float, o_shift: NDArray,
        started: list[bool], call_no: int,
    ) -> NDArray:
        """Solve the layer coefficient system for a given α.

        Tries PartInvert first, falls back to LU then Gauss-Jordan.
        For rigid-type designs, always uses Gauss-Jordan.
        """
        rigid_types = ("FlexOnRigid", "NewRigid", "UnbondOnRigid",
                       "PartBondOnRigid", "FlexOnFlex")
        use_gj = self._design_type in rigid_types or self._design_type is None

        if use_gj:
            b, _ = self._find_constants_full(alpha_by_z, o_shift, SolverType.GAUSS_JORDAN)
            # Sanitize NaN/Inf
            b = np.where(np.isfinite(b), b, 0.0)
            return b

        if not started[call_no]:
            self._pi_failed[call_no] = False
            self._lu_failed[call_no] = False
            started[call_no] = True

        b = None
        if not self._pi_failed[call_no]:
            b, bad = self._find_constants_part_invert(alpha_by_z, o_shift)
            if bad == -1:
                self._pi_failed[call_no] = True

        if self._pi_failed[call_no] and not self._lu_failed[call_no]:
            b, bad = self._find_constants_full(alpha_by_z, o_shift, SolverType.LU)
            if bad == -1:
                self._lu_failed[call_no] = True

        if self._pi_failed[call_no] and self._lu_failed[call_no]:
            b, _ = self._find_constants_full(alpha_by_z, o_shift, SolverType.GAUSS_JORDAN)

        return b  # type: ignore[return-value]

    # ── PartInvert solver ─────────────────────────────────────────────────

    def _find_constants_part_invert(
        self, alpha: float, o_shift: NDArray
    ) -> tuple[NDArray, int]:
        """Fast partial-inversion solver (no pivoting).

        Returns (B, bad_condition) where B is 1-based coefficient array
        and bad_condition is -1 on failure.
        """
        nl = self._n_layers
        nn = 4 * nl
        A = np.zeros((nn + 1, nn + 1))
        B = np.zeros(nn + 3)  # extra room for subgrade zero insertion
        ATI1 = np.zeros((5, 5))
        A11A12 = np.zeros((nn + 1, 3))
        A11 = np.zeros((nn + 1, nn + 1))

        try:
            exp1 = math.exp(-alpha * o_shift[1, 1])
            exp2 = math.exp(-alpha * o_shift[1, 2])
        except OverflowError:
            return B, -1

        # Surface vertical stress
        p1 = self._poissons[1]
        A[1, 1] = exp1;  A[1, 2] = -exp2
        A[1, 3] = -(1 - 2 * p1) * exp1
        A[1, 4] = -(1 - 2 * p1) * exp2
        # Surface shear stress
        A[2, 1] = exp1;  A[2, 2] = exp2
        A[2, 3] = 2 * p1 * exp1
        A[2, 4] = -2 * p1 * exp2

        for i in range(1, nl):
            try:
                exp1 = math.exp(-alpha * (-self._h_layer[i] + o_shift[i, 1]))
                exp2 = math.exp(-alpha * (self._h_layer[i] + o_shift[i, 2]))
                exp3 = math.exp(-alpha * o_shift[i + 1, 1])
                exp4 = math.exp(-alpha * o_shift[i + 1, 2])
            except OverflowError:
                return B, -1

            az1 = alpha * self._h_layer[i]
            k = (i - 1) * 4
            pi_ = self._poissons[i]
            pi1 = self._poissons[i + 1]

            # Lower-layer elements
            j = i * 4 - 1
            A[j, k+5] = -exp3;  A[j, k+6] = exp4
            A[j, k+7] = (1 - 2*pi1)*exp3;  A[j, k+8] = (1 - 2*pi1)*exp4

            j += 1  # shear stress
            A[j, k+5] = -exp3;  A[j, k+6] = -exp4
            A[j, k+7] = -(2*pi1)*exp3;  A[j, k+8] = (2*pi1)*exp4

            R = (1+pi1)*self._youngs[i] / (self._youngs[i+1]*(1+pi_))
            j += 1  # vertical displacement
            A[j, k+5] = -exp3*R;  A[j, k+6] = -exp4*R
            A[j, k+7] = (2-4*pi1)*exp3*R;  A[j, k+8] = -(2-4*pi1)*exp4*R

            j += 1  # radial displacement / interface shear
            A[j, k+5] = -exp3*R;  A[j, k+6] = exp4*R
            A[j, k+7] = -exp3*R;  A[j, k+8] = -exp4*R

            # Interface shear combination
            iparm = max(self._interface_parm[i], 0.001**4)
            fact1_shear = -alpha * (1 - iparm) * self._youngs[i] / (1 + pi_)
            fact2_shear = iparm
            A[j, k+5] = -A[j, k+5]*fact2_shear
            A[j, k+6] = -A[j, k+6]*fact2_shear
            A[j, k+7] = -A[j, k+7]*fact2_shear
            A[j, k+8] = -A[j, k+8]*fact2_shear

            # Upper layer inverse
            alpha_g = -alpha * (1 - iparm) * self._youngs[i] / (1 + pi_)
            f1 = 1.0 / (4.0 * iparm * (1 - pi_))
            f2 = f1 / exp2 if exp2 != 0 else 0.0
            f1 = f1 / exp1 if exp1 != 0 else 0.0

            A[j-3, k+1] = iparm*(1+az1)*f1
            A[j-3, k+2] = (alpha_g*(1-2*pi_-az1) + iparm*(2-4*pi_-az1))*f1
            A[j-3, k+3] = iparm*(2*pi_+az1)*f1
            A[j-3, k+4] = -(1-2*pi_-az1)*f1

            A[j-2, k+1] = -iparm*(1-az1)*f2
            A[j-2, k+2] = (-alpha_g*(1-2*pi_+az1) + iparm*(2-4*pi_+az1))*f2
            A[j-2, k+3] = iparm*(2*pi_-az1)*f2
            A[j-2, k+4] = (1-2*pi_+az1)*f2

            A[j-1, k+1] = -iparm*f1
            A[j-1, k+2] = (alpha_g+iparm)*f1
            A[j-1, k+3] = -iparm*f1
            A[j-1, k+4] = -f1

            A[j, k+1] = -iparm*f2
            A[j, k+2] = (alpha_g-iparm)*f2
            A[j, k+3] = iparm*f2
            A[j, k+4] = -f2

            # Post-multiply: upper-inverse × lower
            for ii in range(1, 5):
                n = j - 4 + ii
                for jj in range(1, 5):
                    kk = k + jj + 4
                    ATI1[ii, jj] = (A[n, k+1]*A[j-3, kk] + A[n, k+2]*A[j-2, kk]
                                  + A[n, k+3]*A[j-1, kk] + A[n, k+4]*A[j, kk])

            for ii in range(1, 5):
                n = j - 4 + ii
                A[n, k+1] = 0.0;  A[n, k+2] = 0.0
                A[n, k+3] = 0.0;  A[n, k+4] = 0.0
                A[n, k+5] = ATI1[ii, 1];  A[n, k+6] = ATI1[ii, 2]
                A[n, k+7] = ATI1[ii, 3];  A[n, k+8] = ATI1[ii, 4]

        # Reduce to (4N-2)×(4N-2) system
        K = 4 * nl - 2
        if nl > 1:
            for i in range(K-3, K+1):
                A[i, K-1] = A[i, K]
                A[i, K] = A[i, K+2]
        else:
            A[1, 1] = A[1, 2];  A[1, 2] = A[1, 4]
            A[2, 1] = A[2, 2];  A[2, 2] = A[2, 4]

        KK = K - 2
        for i in range(1, KK+1):
            for j in range(1, KK+1):
                A11[i, j] = A[i+2, j]

        if nl > 1:
            for i in range(K-3, K+1):
                A11A12[i-2, 1] = -A[i, K-1]
                A11A12[i-2, 2] = -A[i, K]

        # Back-substitution
        for i in range(KK, 0, -1):
            if i <= KK - 4:
                jj = i - ((i-1) % 4) + 4
                A11A12[i, 1] = -(A11[i,jj]*A11A12[jj,1] + A11[i,jj+1]*A11A12[jj+1,1]
                               + A11[i,jj+2]*A11A12[jj+2,1] + A11[i,jj+3]*A11A12[jj+3,1])
                A11A12[i, 2] = -(A11[i,jj]*A11A12[jj,2] + A11[i,jj+1]*A11A12[jj+1,2]
                               + A11[i,jj+2]*A11A12[jj+2,2] + A11[i,jj+3]*A11A12[jj+3,2])

        # Form 2×2 Schur complement (AStar)
        as11 = as12 = as21 = as22 = 0.0
        if nl > 1:
            for i in range(1, 5):
                as11 += A[1, i]*A11A12[i, 1]
                as12 += A[1, i]*A11A12[i, 2]
                as21 += A[2, i]*A11A12[i, 1]
                as22 += A[2, i]*A11A12[i, 2]
        else:
            as11 = A[1, 1]; as12 = A[1, 2]
            as21 = A[2, 1]; as22 = A[2, 2]

        # Solve for last two coefficients
        TINY = 1e-17
        dtemp = as11 - as12 * (as21 / as22) if as22 != 0 else TINY
        if dtemp == 0:
            dtemp = TINY
        if abs(dtemp) <= TINY:
            B[KK + 1] = 0.0
        else:
            B[KK + 1] = 1.0 / dtemp

        B[KK + 2] = -B[KK + 1] * as21 / as22 if as22 != 0 else 0.0

        # Residual check
        r1 = as11 * B[KK+1] + as12 * B[KK+2] - 1.0
        r2 = as21 * B[KK+1] + as22 * B[KK+2]
        bad = -1 if math.sqrt(r1*r1 + r2*r2) / 2.0 > 1e-4 else 0

        # Remaining coefficients
        for i in range(1, KK+1):
            B[i] = A11A12[i, 1]*B[KK+1] + A11A12[i, 2]*B[KK+2]

        # Insert zeros for infinite subgrade (A_last=0, C_last=0)
        B[K+2] = B[K]
        B[K+1] = 0.0
        B[K] = B[K-1]
        B[K-1] = 0.0

        return B, bad

    # ── Full-matrix solver ────────────────────────────────────────────────

    def _find_constants_full(
        self, alpha: float, o_shift: NDArray, solver: SolverType
    ) -> tuple[NDArray, int]:
        """Full matrix assembly + direct solve (GJ, LU, or Gauss)."""
        nl = self._n_layers
        nn = 4 * nl
        A = np.zeros((nn + 1, nn + 1))
        B = np.zeros(nn + 3)

        try:
            exp1 = math.exp(-alpha * o_shift[1, 1])
            exp2 = math.exp(-alpha * o_shift[1, 2])
        except OverflowError:
            return B, -1

        # Surface BCs
        p1 = self._poissons[1]
        A[1,1] = exp1; A[1,2] = -exp2
        A[1,3] = -(1-2*p1)*exp1; A[1,4] = -(1-2*p1)*exp2
        A[2,1] = exp1; A[2,2] = exp2
        A[2,3] = 2*p1*exp1; A[2,4] = -2*p1*exp2

        for i in range(1, nl):
            try:
                exp1 = math.exp(-alpha*(-self._h_layer[i]+o_shift[i,1]))
                exp2 = math.exp(-alpha*(self._h_layer[i]+o_shift[i,2]))
                exp3 = math.exp(-alpha*o_shift[i+1,1])
                exp4 = math.exp(-alpha*o_shift[i+1,2])
            except OverflowError:
                return B, -1

            az1 = alpha * self._h_layer[i]
            k = (i-1)*4
            pi_ = self._poissons[i]
            pi1 = self._poissons[i+1]

            j = i*4 - 1
            # Vertical stress continuity
            A[j,k+1]=exp1; A[j,k+2]=-exp2
            A[j,k+3]=-(1-2*pi_-az1)*exp1; A[j,k+4]=-(1-2*pi_+az1)*exp2
            A[j,k+5]=-exp3; A[j,k+6]=exp4
            A[j,k+7]=(1-2*pi1)*exp3; A[j,k+8]=(1-2*pi1)*exp4

            j += 1  # Shear stress
            A[j,k+1]=exp1; A[j,k+2]=exp2
            A[j,k+3]=(2*pi_+az1)*exp1; A[j,k+4]=-(2*pi_-az1)*exp2
            A[j,k+5]=-exp3; A[j,k+6]=-exp4
            A[j,k+7]=-(2*pi1)*exp3; A[j,k+8]=(2*pi1)*exp4

            R = (1+pi1)*self._youngs[i]/(self._youngs[i+1]*(1+pi_))
            j += 1  # Vertical displacement
            A[j,k+1]=exp1; A[j,k+2]=exp2
            A[j,k+3]=-(2-4*pi_-az1)*exp1; A[j,k+4]=(2-4*pi_+az1)*exp2
            A[j,k+5]=-exp3*R; A[j,k+6]=-exp4*R
            A[j,k+7]=(2-4*pi1)*exp3*R; A[j,k+8]=-(2-4*pi1)*exp4*R

            j += 1  # Interface shear / radial displacement
            alpha_g = -alpha * (1 - self._interface_parm[i])
            r1 = self._interface_parm[i]*(1+pi_)/self._youngs[i]
            r2 = self._interface_parm[i]*(1+pi1)/self._youngs[i+1]
            A[j,k+1]=(alpha_g-r1)*exp1; A[j,k+2]=(alpha_g+r1)*exp2
            A[j,k+3]=(alpha_g*(2*pi_+az1)-(1+az1)*r1)*exp1
            A[j,k+4]=(-alpha_g*(2*pi_-az1)-(1-az1)*r1)*exp2
            A[j,k+5]=r2*exp3; A[j,k+6]=-r2*exp4
            A[j,k+7]=r2*exp3; A[j,k+8]=r2*exp4

        # Reduce for infinite subgrade (A_last=C_last=0)
        K = 4*nl - 2
        if nl > 1:
            for i in range(K-3, K+1):
                A[i, K-1] = A[i, K]
                A[i, K] = A[i, K+2]
        else:
            A[1,1]=A[1,2]; A[1,2]=A[1,4]
            A[2,1]=A[2,2]; A[2,2]=A[2,4]

        # RHS: surface load = [1, 0, 0, ...]
        for i in range(1, K+1):
            B[i] = 0.0
        B[1] = 1.0

        # Solve (convert to 0-based for numpy solvers)
        A0 = A[1:K+1, 1:K+1].copy()
        B0 = B[1:K+1].copy()
        ifail = 0

        if solver == SolverType.GAUSS_JORDAN:
            ifail = gauss_jordan(A0, B0)
        elif solver == SolverType.LU:
            ifail = lu_solve(A0, B0)
        else:
            ifail = gauss_jordan(A0, B0)  # default fallback

        # Copy back to 1-based
        B[1:K+1] = B0

        # Insert subgrade zeros
        B[K+2] = B[K]
        B[K+1] = 0.0
        B[K] = B[K-1]
        B[K-1] = 0.0

        return B, ifail

    # ── Vertical strain integration ───────────────────────────────────────

    def _integrate_z_strain(
        self, eval_layer: int, z_eval: float
    ) -> NDArray[np.float64]:
        """Compute vertical strain at evaluation depth via Hankel inverse."""
        nac = self._nac
        n_eval_max, n_tires_max, r_max = self._get_max_parms()
        nl = self._n_layers

        strain_w = np.zeros((nac + 1, n_eval_max + 1))
        ac_converged = np.zeros(nac + 1, dtype=bool)
        nac_converged = 0

        il = eval_layer
        z_layer = z_eval - self._z_interface[il - 1]
        o_shift = self._set_o_shifts(il, z_layer, r_max)

        z1 = o_shift[il, 1] - z_layer
        z2 = o_shift[il, 2] + z_layer
        zl1 = z_layer / z1
        zl2 = z_layer / z2
        pois_x2 = self._poissons[eval_layer] * 2
        i_const = (eval_layer - 1) * 4

        started = [False, False, False]

        for ig in range(1, self._gl_n + 1):
            strain_wig_conv = 0.0
            strain_wig1 = 0.0

            if eval_layer < nl:
                b = self._find_constants(
                    self._gl_alpha[ig] / z1, o_shift, started, 1)
                ak = b[i_const + 1]
                alpha_z = self._gl_alpha[ig] * zl1
                ck = b[i_const + 3] * (1 - pois_x2*2 - alpha_z)
                strain_wig1 = -(ak - ck) * self._gl_weight[ig] / z1
                strain_wig_conv = (abs(ak)+abs(ck)) * self._gl_weight[ig] / z1

            b = self._find_constants(
                self._gl_alpha[ig] / z2, o_shift, started, 2)
            bk = b[i_const + 2]
            alpha_z = self._gl_alpha[ig] * zl2
            dk = b[i_const + 4] * (1 - pois_x2*2 + alpha_z)
            strain_wig2 = (bk + dk) * self._gl_weight[ig] / z2
            strain_wig_conv += (abs(bk)+abs(dk)) * self._gl_weight[ig] / z2

            for iac in range(1, nac + 1):
                if ac_converged[iac]:
                    continue
                a2 = self._tire_radius[iac, 1]
                a1 = a2 / z1
                a2_s = a2 / z2

                j1a1 = 0.0
                if eval_layer < nl:
                    j1a1 = self._load_function(self._gl_alpha[ig], a1) * strain_wig1
                j1a2 = self._load_function(self._gl_alpha[ig], a2_s) * strain_wig2

                nt = self._n_tires[iac]
                ne = self._n_eval_pts[iac]
                r_all = self._radius[iac, 1:nt+1, 1:ne+1]
                alpha_ig = self._gl_alpha[ig]

                j0r1_all = _bessj0_vec(alpha_ig * r_all / z1) if eval_layer < nl else 0.0
                j0r2_all = _bessj0_vec(alpha_ig * r_all / z2)

                strain_w[iac, 1:ne+1] += np.sum(
                    j0r1_all * j1a1 + j0r2_all * j1a2, axis=0)

                sw_min = np.min(np.abs(strain_w[iac, 1:ne+1]))
                if sw_min > 0 and strain_wig_conv / sw_min < self._convergence_limit:
                    ac_converged[iac] = True
                    nac_converged += 1

            if nac_converged == nac:
                break

        for iac in range(1, nac + 1):
            factor = (self._tire_press[iac, 1] * self._tire_radius[iac, 1]
                      * (1 + self._poissons[eval_layer]) / self._youngs[eval_layer])
            ne = self._n_eval_pts[iac]
            strain_w[iac, 1:ne+1] *= factor

        return strain_w

    # ── Horizontal stress integration ─────────────────────────────────────

    def _integrate_h_stress(
        self, eval_layer: int, z_eval: float
    ) -> NDArray[np.float64]:
        """Compute max horizontal stress at evaluation depth."""
        nac = self._nac
        n_eval_max, n_tires_max, r_max = self._get_max_parms()
        nl = self._n_layers

        response = np.zeros((nac+1, n_eval_max+1))
        stress_r = np.zeros((nac+1, n_tires_max+1, n_eval_max+1))
        stress_t = np.zeros((nac+1, n_tires_max+1, n_eval_max+1))
        ac_converged = np.zeros(nac+1, dtype=bool)
        nac_converged = 0

        il = eval_layer
        z_layer = z_eval - self._z_interface[il-1]
        o_shift = self._set_o_shifts(il, z_layer, r_max)

        z1 = o_shift[il, 1] - z_layer
        z2 = o_shift[il, 2] + z_layer
        zl1 = z_layer / z1
        zl2 = z_layer / z2
        pois_x2 = self._poissons[eval_layer] * 2
        i_const = (eval_layer - 1) * 4

        started = [False, False, False]

        for ig in range(1, self._gl_n + 1):
            response_conv = 0.0

            b1 = self._find_constants(self._gl_alpha[ig]/z1, o_shift, started, 1)
            ak = b1[i_const+1]; ck = b1[i_const+3]
            glw_z1 = self._gl_weight[ig] / z1
            srt01 = pois_x2 * ck * glw_z1
            response_conv = pois_x2 * abs(ck)
            ck_mod = ck * (1 + self._gl_alpha[ig]*zl1)
            srt11 = (ak + ck_mod) * glw_z1
            response_conv = (response_conv + abs(ak) + abs(ck_mod)) * glw_z1

            b2 = self._find_constants(self._gl_alpha[ig]/z2, o_shift, started, 2)
            bk = b2[i_const+2]; dk = b2[i_const+4]
            glw_z2 = self._gl_weight[ig] / z2
            srt02 = pois_x2 * dk * glw_z2
            response_conv += pois_x2 * abs(dk) * glw_z2
            dk_mod = dk * (1 - self._gl_alpha[ig]*zl2)
            srt12 = -(bk - dk_mod) * glw_z2
            response_conv += (abs(bk) + abs(dk_mod)) * glw_z2

            for iac in range(1, nac+1):
                if ac_converged[iac]:
                    continue

                a2 = self._tire_radius[iac, 1]
                a1 = a2/z1; a2_s = a2/z2
                j1a1 = self._load_function(self._gl_alpha[ig], a1) if eval_layer < nl else 0.0
                j1a2 = self._load_function(self._gl_alpha[ig], a2_s)

                srt01_j = srt01*j1a1; srt11_j = srt11*j1a1
                srt02_j = srt02*j1a2; srt12_j = srt12*j1a2

                nt = self._n_tires[iac]
                ne = self._n_eval_pts[iac]
                r_all = self._radius[iac, 1:nt+1, 1:ne+1]
                alpha_ig = self._gl_alpha[ig]

                if eval_layer < nl:
                    ar1 = alpha_ig * r_all / z1
                    j0r1_all = _bessj0_vec(ar1)
                    j1r1_all = _bessj1_over_x_vec(ar1)
                else:
                    j0r1_all = np.zeros_like(r_all)
                    j1r1_all = np.zeros_like(r_all)

                ar2 = alpha_ig * r_all / z2
                j0r2_all = _bessj0_vec(ar2)
                j1r2_all = _bessj1_over_x_vec(ar2)

                sr = (j0r1_all*srt01_j + j0r2_all*srt02_j
                      + (j0r1_all - j1r1_all)*srt11_j
                      + (j0r2_all - j1r2_all)*srt12_j)
                st = (j0r1_all*srt01_j + j0r2_all*srt02_j
                      + j1r1_all*srt11_j + j1r2_all*srt12_j)

                stress_r[iac, 1:nt+1, 1:ne+1] += sr
                stress_t[iac, 1:nt+1, 1:ne+1] += st

                temp_all = np.maximum(
                    np.abs(stress_r[iac, 1:nt+1, 1:ne+1]),
                    np.abs(stress_t[iac, 1:nt+1, 1:ne+1]))
                temp_min = max(np.min(temp_all), 1e-12)
                if abs(response_conv / temp_min) < self._convergence_limit:
                    ac_converged[iac] = True
                    nac_converged += 1

            if nac_converged == nac:
                break

        for iac in range(1, nac+1):
            factor = (self._tire_press[iac,1] * self._tire_radius[iac,1]
                      * (1+self._poissons[eval_layer]) / self._youngs[eval_layer])
            nt = self._n_tires[iac]
            ne = self._n_eval_pts[iac]

            ex = self._eval_x[iac, 1:ne+1]
            ey = self._eval_y[iac, 1:ne+1]
            tx = self._tire_x[iac, 1:nt+1]
            ty = self._tire_y[iac, 1:nt+1]

            dx = ex[np.newaxis, :] - tx[:, np.newaxis]
            dy = ey[np.newaxis, :] - ty[:, np.newaxis]
            r = self._radius[iac, 1:nt+1, 1:ne+1]

            with np.errstate(divide="ignore", invalid="ignore"):
                cos_sq = np.where(r > 1e-10, (dx / r) ** 2, 0.5)
                sin_sq = np.where(r > 1e-10, (dy / r) ** 2, 0.5)

            sr_f = stress_r[iac, 1:nt+1, 1:ne+1] * factor
            st_f = stress_t[iac, 1:nt+1, 1:ne+1] * factor

            sx = np.sum(sr_f * cos_sq + st_f * sin_sq, axis=0)
            sy = np.sum(sr_f * sin_sq + st_f * cos_sq, axis=0)

            mask = np.abs(sx) >= np.abs(sy)
            response[iac, 1:ne+1] = np.where(mask, sx, sy)

        return response

    # ── Vertical deflection integration ───────────────────────────────────

    def _integrate_z_deflection(
        self, eval_layer: int, z_eval: float
    ) -> NDArray[np.float64]:
        """Compute vertical deflection at evaluation depth."""
        nac = self._nac
        n_eval_max, n_tires_max, r_max = self._get_max_parms()
        nl = self._n_layers

        defl_w = np.zeros((nac+1, n_eval_max+1))
        ac_converged = np.zeros(nac+1, dtype=bool)
        nac_converged = 0

        il = eval_layer
        z_layer = z_eval - self._z_interface[il-1]
        o_shift = self._set_o_shifts(il, z_layer, r_max)

        z1 = o_shift[il,1] - z_layer
        z2 = o_shift[il,2] + z_layer
        zl1 = z_layer / z1
        zl2 = z_layer / z2
        pois_x2 = self._poissons[eval_layer] * 2
        i_const = (eval_layer-1)*4

        started = [False, False, False]

        for ig in range(1, self._gl_n + 1):
            defl_conv = 0.0
            defl_wig1 = 0.0

            if eval_layer < nl:
                b = self._find_constants(self._gl_alpha[ig]/z1, o_shift, started, 1)
                ak = b[i_const+1]; ck = b[i_const+3]
                alpha_z = self._gl_alpha[ig]*zl1
                ck_d = ck*(2-pois_x2*2-alpha_z)
                defl_wig1 = -(ak - ck_d)*self._gl_weight[ig]/z1
                defl_conv = (abs(ak)+abs(ck_d))*self._gl_weight[ig]/z1

            b = self._find_constants(self._gl_alpha[ig]/z2, o_shift, started, 2)
            bk = b[i_const+2]; dk = b[i_const+4]
            alpha_z = self._gl_alpha[ig]*zl2
            dk_d = dk*(2-pois_x2*2+alpha_z)
            defl_wig2 = -(bk + dk_d)*self._gl_weight[ig]/z2
            defl_conv += (abs(bk)+abs(dk_d))*self._gl_weight[ig]/z2

            for iac in range(1, nac+1):
                if ac_converged[iac]:
                    continue
                a2 = self._tire_radius[iac,1]
                a1 = a2/z1; a2_s = a2/z2
                j1a1 = self._load_function(self._gl_alpha[ig], a1)*defl_wig1 if eval_layer < nl else 0.0
                j1a2 = self._load_function(self._gl_alpha[ig], a2_s)*defl_wig2

                nt = self._n_tires[iac]
                ne = self._n_eval_pts[iac]
                r_all = self._radius[iac, 1:nt+1, 1:ne+1]
                alpha_ig = self._gl_alpha[ig]

                j0r1_all = _bessj0_vec(alpha_ig * r_all / z1) if eval_layer < nl else 0.0
                j0r2_all = _bessj0_vec(alpha_ig * r_all / z2)

                defl_w[iac, 1:ne+1] += np.sum(
                    j0r1_all * j1a1 + j0r2_all * j1a2, axis=0)

                dw_min = np.min(np.abs(defl_w[iac, 1:ne+1]))
                if dw_min > 0 and defl_conv / dw_min < self._convergence_limit:
                    ac_converged[iac] = True
                    nac_converged += 1

            if nac_converged == nac:
                break

        for iac in range(1, nac+1):
            factor = (self._tire_press[iac,1] * self._tire_radius[iac,1]
                      * (1+self._poissons[eval_layer]) / self._youngs[eval_layer])
            ne = self._n_eval_pts[iac]
            defl_w[iac, 1:ne+1] *= factor

        return defl_w
