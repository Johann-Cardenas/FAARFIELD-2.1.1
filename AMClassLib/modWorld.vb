' change "Single" to "Double", "CSng" to "CDbl" for FAASR3D by YC 102418-041519

Option Strict On
Option Explicit On

Module modWorld

    Declare Function timeGetTime Lib "winmm" () As Integer
    'Declare Sub Ingrid Lib "C:\0FEDFAA.February\INGRID\Debug\ingrid.dll" ()
    'Declare Sub Ingrid Lib "C:\0FEDFAA.February\FEDFAA.NET\bin\ingrid.dll" ()


    Public gDesignType2 As Integer 'Izydor Kawa March 31, 2014

    'Declare Sub NIKE3D Lib "C:\0FEDFAA.February\NIKE3D\Debug\nike3d.dll" (ByRef Stress1 As Double, ByRef Stress8 As Double, ByRef StopFEDFAA As Short, ByRef FEDFAAStopped As Short, ByRef FEDFAAStopped2 As Short, ByRef i1 As Short)
    'Declare Sub NIKE3D Lib "C:\0FEDFAA.February\FEDFAA.NET\bin\nike3d.dll" (ByRef Stress1 As Double, ByRef Stress8 As Double, ByRef StopFEDFAA As Short, ByRef FEDFAAStopped As Short, ByRef FEDFAAStopped2 As Short, ByRef i1 As Short)
    Public gUserInterrupted As Boolean 'September 13, 2006
    Public gDesignType As Integer
    Public iSymCase As Short
    Public MeshCase As String
    Public Const gAC As Integer = 40
    Public GroupIndex(gAC * 2) As Short 'indicates belonging to particular ac group from 1 to 4

    Public SolverType As Integer 'added solver choice (EBE vs Direct) by YGC 083012
    Public SlabMeshSize As Integer 'added slab mesh size selection by YGC 061113

    Public iCat As Short            'counter for group number

    'Public Const gMeshCategories As Integer = 5    'changed to 2 for symmetrical and unsymmetrical gear by YC 082216 092016
    Public Const gMeshCategories As Integer = 2

    Public NCat(gMeshCategories) As Short         'number of AC in each category
    Public firstACCat(gMeshCategories) As Short   'index of first AC in the list for each category
    Public gNACarg As Integer       'global variable for number of AC in the whole list


    'Declare Sub NIKE3D Lib "Nike3d.dll" (ByRef Stress1 As Double, ByRef Stress8 As Double, ByRef StopFEDFAA As Short, ByRef FEDFAAStopped As Short)
    'Declare Sub NIKE3D Lib "Nike3d.dll" (ByRef Stress1 As Double, ByRef Stress8 As Double, ByRef StopFEDFAA As Short, ByRef FEDFAAStopped As Short, ByVal ala As String, ByRef gDesignType As Short)
    'Declare Sub NIKE3D Lib "Nike3d.dll" (ByRef Stress1 As Double, ByRef Stress8 As Double, ByRef StopFEDFAA As Short, ByRef FEDFAAStopped As Short, ByVal ala As String)
    'Declare Sub NIKE3D Lib "Nike3d.dll" (ByRef Stress1 As Double, ByRef Stress8 As Double, ByRef StopFEDFAA As Short, ByRef FEDFAAStopped As Short, ByRef FEDFAAStopped2 As Short, ByRef ala As String)


    'Modified to use printed stress by YGC 112213
    'YGC 101012
    'Declare Sub NIKE3D Lib "Nike3d.dll" (ByRef Stress1 As Double, ByRef Stress8 As Double, ByRef StopFEDFAA As Short, ByRef FEDFAAStopped As Short, ByRef FEDFAAStopped2 As Short, ByRef i1 As Short)

    ' recovered to use transfered stress by YC 052615
    Declare Sub NIKE3D Lib "Nike3d.dll" (ByRef Stress1 As Double, ByRef Stress8 As Double, ByRef StopFEDFAA As Short, ByRef FEDFAAStopped As Short, ByRef FEDFAAStopped2 As Short, ByRef i1 As Short, ByVal WorkingDir0 As String, ByVal LenWorkingDir As Integer)
    'Declare Sub NIKE3D Lib "Nike3d.dll" (ByRef StopFEDFAA As Short, ByRef FEDFAAStopped As Short, ByRef FEDFAAStopped2 As Short, ByRef i1 As Short, ByVal WorkingDir0 As String, ByVal LenWorkingDir As Integer)
    'Modified to use printed stress by YGC 112213 END
    ' recovered to use transfered stress by YC 052615 END

    'YGC 101012


    'Declare Sub N3DFAA Lib "N3DFAA.dll" (ByRef Stress1 As Double, ByRef Stress8 As Double)
    Public Stress1(gAC * 2) As Double
    Public Stress8(gAC * 2) As Double

    Public gNumberOfFoundationInterfaces As Integer 'defined number of foundation interfaces (depth) for rigid compaction by YGC 112213
    Public NSGLayer As Integer = 25 'added for rigid compaction by YGC 112213
    Public stress33(gAC, NSGLayer), zstress33(NSGLayer) As Double 'added to output foundation vertical stress and depth for rigid compaction by YGC 112213

    'YGC 101012
    'Declare Sub INGRIDMAIN Lib "ingrid3.dll" () 

    'Declare Sub INGRIDMAIN Lib "ingrid3.dll" (ByVal WorkingDir0 As String, ByVal LenWorkingDir As Integer) ' add Fortran error handeling by YC 052416
    'Declare Sub INGRIDMAIN Lib "ingrid3.dll" (ByRef IUnitErr As Short, ByRef IErr As Short, ByVal WorkingDir0 As String, ByVal LenWorkingDir As Integer)

    'YGC 101012


    Public Const PI As Double = 3.14159265359

    'Public Factor As Double


    Declare Function GlobalAddAtom Lib "kernel32" Alias "GlobalAddAtomA" (ByVal lpString As String) As Short
    Declare Function GlobalFindAtom Lib "kernel32" Alias "GlobalFindAtomA" (ByVal lpString As String) As Short
    Declare Function GlobalDeleteAtom Lib "kernel32" (ByVal nAtom As Short) As Short
    Declare Function GlobalGetAtomName Lib "kernel32" Alias "GlobalGetAtomNameA" (ByVal nAtom As Short, ByVal lpBuffer As String, ByVal nSize As Integer) As Integer



    Public Sub PIKSTR2(ByVal N As Integer, ByVal ARR(,) As Double, ByRef NewARR(,) As Double)
        Dim I, J As Integer, A As Double, B As Double

        For J = 2 To N
            A = ARR(J, 1)
            B = ARR(J, 2)
            For I = J - 1 To 1 Step -1
                If (ARR(I, 1) <= A) Then
                    GoTo goto10
                End If
                ARR(I + 1, 1) = ARR(I, 1)
                ARR(I + 1, 2) = ARR(I, 2)
            Next I
            I = 0
goto10:     ARR(I + 1, 1) = A
            ARR(I + 1, 2) = B
        Next

        For I = 1 To N
            NewARR(I, 1) = ARR(N - I + 1, 1)
            NewARR(I, 2) = ARR(N - I + 1, 2)
        Next

    End Sub


    Public Sub SortingAscending(ByVal N As Integer, ByVal ARR(,) As Double, ByRef NewARR(,) As Double)      '112017-030719 YC for sorting
        Dim I, J As Integer, A As Double, B As Double

        For J = 2 To N
            A = ARR(J, 1)
            B = ARR(J, 2)
            For I = J - 1 To 1 Step -1
                If (ARR(I, 1) <= A) Then
                    GoTo goto10
                End If
                ARR(I + 1, 1) = ARR(I, 1)
                ARR(I + 1, 2) = ARR(I, 2)
            Next I
            I = 0
goto10:     ARR(I + 1, 1) = A
            ARR(I + 1, 2) = B
        Next

        For I = 1 To N
            NewARR(I, 1) = ARR(I, 1)
            NewARR(I, 2) = ARR(I, 2)
        Next

    End Sub


    Function LPad(ByRef N As Integer, ByVal SS As String) As String
        ' Adds leading spaces to variant string SS to make it N characters long.
        ' Used to format output to a file. #### characters in a Format function
        ' do not force spaces like QuickBasic.
        ' Typically, SS = Format(XX, "0.00")
        Dim ITemp As Integer

        ITemp = Len(SS)
        If N - ITemp < 1 Then N = ITemp + 1
        LPad = Space(N - ITemp) & SS
    End Function


End Module
