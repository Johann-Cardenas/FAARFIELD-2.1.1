Imports FaarFieldAnalysis
Imports FF2.ViewModels


Public Class UpdateManager
    Private Property ViewModel As MainWindowViewModel

    Public Sub New(VM As MainWindowViewModel)
        ViewModel = VM
    End Sub

    Public Sub UpdateStructure()

        Debug.WriteLine("Update Structure Image")

        For i = 0 To ViewModel.CurrentSectionView.Section.Layers.Count - 1
            If ThicknessDesignRun Or LifeCompactionRun Then 'Only change thickness value with Thickness Design and Life/Compaction Run, Le
                If Not ViewModel.CurrentSectionView.Section.Layers(i).Name.Contains("Variable") Then 'Thickness of "Variable" layer will not be changed with any type of Run, Le
                    If ThicknessDesignRun And ViewModel.CurrentSectionView.Section.Layers(i).Name = "P-209 Crushed Aggregate" Then 'Minimum Thickness of P209 after thickness run = 6
                        If ViewModel.FaarFieldFactory.CreateThickness(FEDFAA1.Thick(i + 1), ViewModel.FaarFieldFactory.CreateUsCustomary()).UsCustomary < 6 Then
                            ViewModel.CurrentSectionView.Section.Layers(i).Thickness = ViewModel.FaarFieldFactory.CreateThickness(6, ViewModel.FaarFieldFactory.CreateUsCustomary)
                        Else
                            ViewModel.CurrentSectionView.Section.Layers.Item(i).Thickness = ViewModel.FaarFieldFactory.CreateThickness(FEDFAA1.Thick(i + 1), ViewModel.FaarFieldFactory.CreateUsCustomary)
                        End If
                    Else
                        ViewModel.CurrentSectionView.Section.Layers.Item(i).Thickness = ViewModel.FaarFieldFactory.CreateThickness(FEDFAA1.Thick(i + 1), ViewModel.FaarFieldFactory.CreateUsCustomary)
                    End If
                Else
                    ViewModel.CurrentSectionView.Section.Layers.Item(i).Thickness = ViewModel.FaarFieldFactory.CreateThickness(FEDFAA1.Thick(i + 1), ViewModel.FaarFieldFactory.CreateUsCustomary)
                End If
            End If

            ViewModel.CurrentSectionView.Section.Layers.Item(i).Modulus = ViewModel.FaarFieldFactory.CreateModulus(FEDFAA1.Modulus(i + 1), ViewModel.FaarFieldFactory.CreateUsCustomary)
        Next
        System.Windows.Application.Current.Dispatcher.Invoke(Sub() ViewModel.ProfileImage = ViewModel.DrawProfile())

    End Sub

End Class
