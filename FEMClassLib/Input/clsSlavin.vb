
Partial Public Class clsInput

    Sub slavin()
        ' Combine islavin and slavin
        Dim i, j, js, jm, nrts, nrtm, nsn, nmn, nty, nn As Integer
        ReDim nsf(2, numsv)
        js = 0 : jm = 0
        ifl = 0 : nsntl = 0 : nmntl = 0
        For i = 1 To clsCom.numsv
            nrts = iparm(1, i)
            nrtm = iparm(2, i)
            nsn = iparm(3, i)
            nmn = iparm(4, i)
            nty = iparm(5, i)

            nn = 4 * Math.Max(nrts, nrtm)
            Dim mnn(nn) As Integer
            Call countn(irects, mnn, nsn, nrts)
            For j = 1 To nn
                mnn(j) = 0
            Next j
            Call countn(irectm, mnn, nmn, nrtm)
            iparm(3, i) = nsn
            iparm(4, i) = nmn
            nsf(1, i) = nsn
            nsf(2, i) = nmn

            If (fric(1, i) ^ 2 + fric(2, i) ^ 2 + fric(3, i) ^ 2) <> 0 Then
                ifl = ifl + nsn + nmn
            End If

            If ifd(i) <> 0 Then nifd = nifd + nsn + nmn
            nsmmax = Math.Max(nsn, nmn)
            nsntl = nsntl + nsn
            nmntl = nmntl + nmn
            js = js + nrts
            jm = jm + nrtm
        Next i
        'Call Check2D(irects, 4, nrttls)
    End Sub
   
End Class
