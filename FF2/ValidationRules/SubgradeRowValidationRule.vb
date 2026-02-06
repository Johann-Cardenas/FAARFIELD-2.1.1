
Namespace ValidationRules

    Public Class SubgradRowValidationRule
        Inherits ValidationRule




        Public Overrides Function Validate(ByVal value As Object,
                                           ByVal cultureInfo As System.Globalization.CultureInfo) As ValidationResult

            'Dim bg As BindingGroup
            'bg = CType(value, BindingGroup)
            'Dim m = CType(bg.Items(0), Material)
            'Dim options = CType(bg.Items(1), DesignOptions)



            'Dim msg As String = ""
            'Dim factory As New FaarFieldModelFactory
            'If m.Name = "P-209 Crushed Aggregate" Then
            '    If m.Thickness.UsCustomary < 4 Then
            '        'If AnalysisType.Name = "New Fleixble" Then

            '        If options.MeasurementSystem.GetType Is GetType(UsCustomary) Then


            '            msg = "P-209 Crushed Aggregate cannot be less than 4 inches"
            '            MessageBox.Show(msg)
            '            m.Thickness = factory.CreateThickness(4, factory.CreateUsCustomary)
            '            Return New ValidationResult(False, msg)
            '        End If
            '    End If
            '    If m.Thickness.Metric < 101.6 Then
            '        If options.MeasurementSystem.GetType Is GetType(Metric) Then
            '            msg = "P-209 Crushed Aggregate cannot be less than 101.6 mm"
            '            MessageBox.Show(msg)
            '            m.Thickness = factory.CreateThickness(101.6, factory.CreateMetric)
            '            Return New ValidationResult(False, msg)
            '        End If
            '    End If
            'End If
            'End If


            ''If m.Name = "P-401/P-403 HMA Surface" Then
            ''    If m.Thickness.UsCustomary < 4 Then
            ''        If options.MeasurementSystem.GetType Is GetType(UsCustomary) Then
            ''            msg = "P-401/P-403 HMA Surface cannot be less than 4 inches"
            ''            MessageBox.Show(msg)
            ''            m.Thickness = factory.CreateThickness(4, factory.CreateUsCustomary)

            ''            Return New ValidationResult(False, msg)
            ''        End If
            ''    End If
            ''    If m.Thickness.Metric < 100 Then
            ''        If options.MeasurementSystem.GetType Is GetType(Metric) Then

            ''            msg = "P-401/P-403 HMA Surface cannot be less than 100 mm"
            ''            MessageBox.Show(msg)
            ''            m.Thickness = factory.CreateThickness(100, factory.CreateMetric)
            ''            Return New ValidationResult(False, msg)

            ''        End If

            ''    End If
            ''End If

            'If m.Name = "P-401/P-403 HMA Overlay" Then
            '    If m.Thickness.UsCustomary < 1 Then
            '        If options.MeasurementSystem.GetType Is GetType(UsCustomary) Then
            '            msg = "P-401/P-403 HMA Overlay cannot be less than 2 inch"
            '            MessageBox.Show(msg)
            '            m.Thickness = factory.CreateThickness(2, factory.CreateUsCustomary)
            '            Return New ValidationResult(False, msg)
            '        End If
            '    End If
            '    If m.Thickness.Metric < 50 Then
            '        If options.MeasurementSystem.GetType Is GetType(Metric) Then

            '            msg = "P-401/P-403 HMA Overlay cannot be less than 25 mm"
            '            MessageBox.Show(msg)
            '            m.Thickness = factory.CreateThickness(50, factory.CreateMetric)
            '            Return New ValidationResult(False, msg)

            '        End If

            '    End If
            'End If


            'If m.Name = "variable (flexible)" Then
            '    If m.Modulus.UsCustomary < 150000 Or m.Modulus.UsCustomary > 400000 Then
            '        If options.MeasurementSystem.GetType Is GetType(UsCustomary) Then
            '            msg = "Variable (Flexible) modulus values can be set in the range 150,000 psi to 400,000 psi."
            '            MessageBox.Show(msg)
            '        Else
            '            msg = "Variable (Flexible) modulus values can be set in the range 1034.21 kp/m to 2757.90 kp/m"
            '            MessageBox.Show(msg)
            '        End If
            '        Return New ValidationResult(False, msg)
            '    End If

            'End If

            'If m.Name = "variable (rigid)" Then
            '    If m.Modulus.UsCustomary < 250000 Or m.Modulus.UsCustomary > 700000 Then
            '        If options.MeasurementSystem.GetType Is GetType(UsCustomary) Then
            '            msg = "Variable (Rigid) modulus values can be set in the range 250,000 psi to 700,000 psi."
            '            MessageBox.Show(msg)
            '        Else
            '            msg = "Variable (Rigid) modulus values can be set in the range 1723.69 kp/m - 4826.33 kp/m."
            '            MessageBox.Show(msg)
            '        End If
            '        Return New ValidationResult(False, msg)
            '    End If

            'End If
            Return ValidationResult.ValidResult

        End Function

    End Class
End Namespace