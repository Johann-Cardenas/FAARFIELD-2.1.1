"""
Numerical routines for NanoFlex — faithful port of LEAFClassLib/Numerical.vb.

All polynomial coefficients and algorithm structure are preserved verbatim from
the Numerical Recipes (Fortran→VB.NET) implementations used by FAARFIELD so
that computed responses match to full double-precision fidelity.
"""

from __future__ import annotations

import math
import numpy as np
from numpy.typing import NDArray

# ═══════════════════════════════════════════════════════════════════════════════
#  Bessel functions  (Numerical Recipes rational-polynomial approximations)
# ═══════════════════════════════════════════════════════════════════════════════

def bessj0(x: float) -> float:
    """J₀(x) — Bessel function of the first kind, order zero."""
    if abs(x) < 8.0:
        y = x * x
        num = (57568490574.0 + y * (-13362590354.0 + y * (651619640.7
              + y * (-11214424.18 + y * (77392.33017 + y * -184.9052456)))))
        den = (57568490411.0 + y * (1029532985.0 + y * (9494680.718
              + y * (59272.64853 + y * (267.8532712 + y * 1.0)))))
        return num / den
    ax = abs(x)
    z = 8.0 / ax
    y = z * z
    xx = ax - 0.785398164
    p = (1.0 + y * (-0.001098628627 + y * (0.00002734510407
        + y * (-0.000002073370639 + y * 0.0000002093887211))))
    q = (-0.01562499995 + y * (0.0001430488765 + y * (-0.000006911147651
        + y * (0.0000007621095161 + y * -0.0000000934945152))))
    return math.sqrt(0.636619772 / ax) * (math.cos(xx) * p - z * math.sin(xx) * q)


def bessj1(x: float) -> float:
    """J₁(x) — Bessel function of the first kind, order one."""
    if abs(x) < 8.0:
        y = x * x
        num = x * (72362614232.0 + y * (-7895059235.0 + y * (242396853.1
              + y * (-2972611.439 + y * (15704.4826 + y * -30.16036606)))))
        den = (144725228442.0 + y * (2300535178.0 + y * (18583304.74
              + y * (99447.43394 + y * (376.9991397 + y * 1.0)))))
        return num / den
    ax = abs(x)
    z = 8.0 / ax
    y = z * z
    xx = ax - 2.356194491
    p = (1.0 + y * (0.00183105 + y * (-0.00003516396496
        + y * (0.000002457520174 + y * -0.000000240337019))))
    q = (0.04687499995 + y * (-0.0002002690873 + y * (0.000008449199096
        + y * (-0.00000088228987 + y * 0.000000105787412))))
    return math.sqrt(0.636619772 / ax) * (math.cos(xx) * p - z * math.sin(xx) * q) * (1 if x >= 0 else -1)


def bessj1_over_x(x: float) -> float:
    """J₁(x)/x — avoids division by zero when x→0.

    For |x|<8 the polynomial evaluates to J₁(x)/x directly (the leading
    factor of *x* in the J₁ numerator is dropped).
    """
    if abs(x) < 8.0:
        y = x * x
        num = (72362614232.0 + y * (-7895059235.0 + y * (242396853.1
              + y * (-2972611.439 + y * (15704.4826 + y * -30.16036606)))))
        den = (144725228442.0 + y * (2300535178.0 + y * (18583304.74
              + y * (99447.43394 + y * (376.9991397 + y * 1.0)))))
        return num / den
    # Large-|x| branch: compute J₁(x) then divide
    return bessj1(x) / x


# Vectorised wrappers for use inside integration loops
_bessj0_vec = np.vectorize(bessj0, otypes=[np.float64])
_bessj1_vec = np.vectorize(bessj1, otypes=[np.float64])
_bessj1_over_x_vec = np.vectorize(bessj1_over_x, otypes=[np.float64])


# ═══════════════════════════════════════════════════════════════════════════════
#  Gauss–Laguerre quadrature
# ═══════════════════════════════════════════════════════════════════════════════

def gammln(xx: float) -> float:
    """ln Γ(xx) — Numerical Recipes Lanczos approximation (6 terms)."""
    cof = (76.1800917294715, -86.5053203294168, 24.0140982408309,
           -1.23173957245015, 0.00120865097386618, -0.000005395239384953)
    x = xx
    y = x
    tmp = x + 5.5
    tmp = (x + 0.5) * math.log(tmp) - tmp
    ser = 1.00000000019001
    for c in cof:
        y += 1.0
        ser += c / y
    return tmp + math.log(2.506628274631 * ser / x)


def gaulag(n: int, alf: float = 0.0) -> tuple[NDArray[np.float64], NDArray[np.float64], int]:
    """Gauss–Laguerre quadrature nodes and weights.

    Returns (x, w, n_actual) where *n_actual* ≤ *n* — the algorithm truncates
    when weights fall below 1e-300 (same as Numerical.vb).
    """
    EPS = 3.0e-14
    MAXIT = 10
    x = np.zeros(n + 1)
    w = np.zeros(n + 1)

    for i in range(1, n + 1):
        # Initial guess for root i
        if i == 1:
            z = (1.0 + alf) * (3.0 + 0.92 * alf) / (1.0 + 2.4 * n + 1.8 * alf)
        elif i == 2:
            z += (15.0 + 6.25 * alf) / (1.0 + 0.9 * alf + 2.5 * n)
        else:
            ai = i - 2
            z += ((1.0 + 2.55 * ai) / (1.9 * ai) + 1.26 * ai * alf / (1.0 + 3.5 * ai)) * (z - x[i - 2]) / (1.0 + 0.3 * alf)

        # Newton–Raphson refinement
        for _ in range(MAXIT):
            p1, p2 = 1.0, 0.0
            for j in range(1, n + 1):
                p3 = p2
                p2 = p1
                p1 = ((2 * j - 1 + alf - z) * p2 - (j - 1 + alf) * p3) / j
            pp = (n * p1 - (n + alf) * p2) / z
            z1 = z
            z = z1 - p1 / pp
            if abs(z - z1) <= EPS:
                break

        x[i] = z
        w[i] = -math.exp(gammln(alf + n) - gammln(float(n))) / (pp * n * p2)
        if w[i] < 1.0e-300 and i > 1:
            return x[1:i], w[1:i], i - 1

    return x[1:], w[1:], n


# ═══════════════════════════════════════════════════════════════════════════════
#  Linear algebra solvers
# ═══════════════════════════════════════════════════════════════════════════════

def gauss_eliminate(a: NDArray[np.float64], r: NDArray[np.float64]) -> int:
    """Gauss elimination with back-substitution (matches Numerical.vb GAUSS).

    Solves A·x = r *in place* — solution overwrites *r*.
    Returns 0 on success, 1 if a near-zero pivot is encountered.
    """
    n = len(r)
    ifail = 0
    pivot = a[0, 0]

    for i in range(n - 1):
        for ii in range(i + 1, n):
            if a[ii, i] != 0.0:
                fact = a[ii, i] / pivot
                r[ii] -= fact * r[i]
                a[ii, i + 1:n] -= fact * a[i, i + 1:n]
                a[ii, i] = 0.0
        pivot = a[i + 1, i + 1]
        if abs(pivot) < 1e-6:
            ifail = 1

    # Back-substitution
    for i in range(n - 1, -1, -1):
        gash = r[i] - np.dot(a[i, i + 1:n], r[i + 1:n])
        r[i] = gash / a[i, i]

    return ifail


def lu_decompose(a: NDArray[np.float64]) -> tuple[NDArray[np.float64], NDArray[np.int32], float, int]:
    """LU decomposition with partial pivoting (Numerical Recipes ludcmp).

    Returns (a_lu, index, d, ifail).  *a* is overwritten in-place.
    """
    n = a.shape[0]
    TINY = 1.0e-20
    indx = np.zeros(n, dtype=np.int32)
    d = 1.0
    ifail = 0

    # Implicit scaling
    vv = np.zeros(n)
    for i in range(n):
        aamax = np.max(np.abs(a[i, :]))
        if aamax == 0.0:
            ifail = -1
            return a, indx, d, ifail
        vv[i] = 1.0 / aamax

    for j in range(n):
        for i in range(j):
            s = a[i, j] - np.dot(a[i, :i], a[:i, j])
            a[i, j] = s

        aamax = 0.0
        imax = j
        for i in range(j, n):
            s = a[i, j] - np.dot(a[i, :j], a[:j, j])
            a[i, j] = s
            dum = vv[i] * abs(s)
            if dum >= aamax:
                imax = i
                aamax = dum

        if j != imax:
            a[[j, imax]] = a[[imax, j]].copy()
            d = -d
            vv[imax] = vv[j]

        indx[j] = imax
        if a[j, j] < TINY ** 2:
            a[j, j] = TINY
            ifail = -1
            return a, indx, d, ifail

        if j != n - 1:
            dum = 1.0 / a[j, j]
            a[j + 1:, j] *= dum

    return a, indx, d, ifail


def lu_back_substitute(a: NDArray[np.float64], indx: NDArray[np.int32],
                       b: NDArray[np.float64]) -> None:
    """LU back-substitution (Numerical Recipes lubksb).  Solves in-place."""
    n = a.shape[0]
    ii = -1
    for i in range(n):
        ll = indx[i]
        s = b[ll]
        b[ll] = b[i]
        if ii >= 0:
            for j in range(ii, i):
                s -= a[i, j] * b[j]
        elif s != 0.0:
            ii = i
        b[i] = s

    for i in range(n - 1, -1, -1):
        s = b[i] - np.dot(a[i, i + 1:n], b[i + 1:n])
        b[i] = s / a[i, i]


def lu_solve(a: NDArray[np.float64], b: NDArray[np.float64]) -> int:
    """Solve A·x = b via LU decomposition (matches Numerical.vb LUsolve).

    Solution overwrites *b*.  Returns 0 on success, -1 on failure.
    """
    n = a.shape[0]
    a_sav = a.copy()
    b_sav = b.copy()

    a, indx, d, ifail = lu_decompose(a)
    if ifail == -1:
        return -1
    lu_back_substitute(a, indx, b)

    # Residual check
    residual = a_sav @ b - b_sav
    rnorm = math.sqrt(np.sum(residual[np.abs(residual) > 1e-50] ** 2))
    if rnorm / n > 1e-4:
        return -1
    return 0


def gauss_jordan(a: NDArray[np.float64], b: NDArray[np.float64],
                 inverse: bool = False, refine: bool = False) -> int:
    """Gauss-Jordan elimination (Numerical Recipes gaussj).

    Solves A*x = b in-place (solution in *b*, inverse in *a* if requested).
    Returns 0 on success, -1 on singular matrix.

    Overflow/invalid warnings are suppressed because the LEAF solver
    intentionally feeds near-singular matrices at high alpha values;
    NaN/Inf results are sanitised by the caller.
    """
    n = a.shape[0]
    indxc = np.zeros(n, dtype=np.int32)
    indxr = np.zeros(n, dtype=np.int32)
    ipiv = np.zeros(n, dtype=np.int32)

    bb = b.copy().reshape(-1, 1)
    aa = a.copy() if refine else None
    b_orig = b.copy() if refine else None

    with np.errstate(over="ignore", invalid="ignore", divide="ignore"):
        for i in range(n):
            big = 0.0
            irow = icol = 0
            for j in range(n):
                if ipiv[j] != 1:
                    for k in range(n):
                        if ipiv[k] == 0:
                            if abs(a[j, k]) >= big:
                                big = abs(a[j, k])
                                irow, icol = j, k
                        elif ipiv[k] > 1:
                            return -1
            ipiv[icol] += 1

            if irow != icol:
                a[[irow, icol]] = a[[icol, irow]].copy()
                bb[[irow, icol]] = bb[[icol, irow]].copy()

            indxr[i] = irow
            indxc[i] = icol

            if a[icol, icol] == 0.0:
                return -1

            pivinv = 1.0 / a[icol, icol]
            a[icol, icol] = 1.0
            a[icol, :] *= pivinv
            bb[icol, :] *= pivinv

            for ll in range(n):
                if ll != icol:
                    dum = a[ll, icol]
                    a[ll, icol] = 0.0
                    a[ll, :] -= a[icol, :] * dum
                    bb[ll, :] -= bb[icol, :] * dum

        if inverse or refine:
            for l in range(n - 1, -1, -1):
                if indxr[l] != indxc[l]:
                    a[:, [indxr[l], indxc[l]]] = a[:, [indxc[l], indxr[l]]].copy()

        if refine and aa is not None and b_orig is not None:
            resid = aa @ bb[:, 0] - b_orig
            correction = a @ resid
            bb[:, 0] -= correction

    b[:] = bb[:, 0]
    return 0


# ═══════════════════════════════════════════════════════════════════════════════
#  Auxiliary
# ═══════════════════════════════════════════════════════════════════════════════

def pythag(a: float, b: float) -> float:
    """√(a² + b²) without overflow (Numerical Recipes)."""
    absa, absb = abs(a), abs(b)
    if absa > absb:
        return absa * math.sqrt(1.0 + (absb / absa) ** 2)
    if absb == 0.0:
        return 0.0
    return absb * math.sqrt(1.0 + (absa / absb) ** 2)
