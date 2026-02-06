Option Strict Off
Option Explicit On
Imports System


Module modPCN_Testgbl

    Public Function LPad1(ByRef N As Double, ByRef SS As String) As String
        ' Adds leading spaces to string SS$ to make it N characters long.
        ' Used to format output to a file. #### characters in a Format function
        ' do not force spaces like QuickBasic.
        ' Typically, SS = Format(XX, "0.00")
        Dim ITemp As Integer = SS.Length
        If N - ITemp < 0 Then N = ITemp + 1
        Return New String(" "c, N - ITemp) & SS
    End Function

    Public Sub Continue_Renamed()
        ' Dummy statement
    End Sub
End Module

