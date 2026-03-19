Imports System.Globalization
Imports FF2.ViewModels

Namespace Converters

    ''' <summary>
    ''' Converts tree node ViewModel types to Unicode icon characters for display in Segoe UI.
    ''' </summary>
    Public NotInheritable Class TreeNodeIconConverter
        Implements IValueConverter

        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            Select Case True
                Case TypeOf value Is JobViewModel
                    Return ChrW(&H25A0) ' ■ black square (job)
                Case TypeOf value Is SectionFolderViewModel
                    Return ChrW(&H25B8) ' ▸ right-pointing small triangle (sections folder)
                Case TypeOf value Is SectionViewModel
                    Return ChrW(&H25FB) ' ◻ white medium square (structure)
                Case TypeOf value Is ReportFolderViewModel
                    Return ChrW(&H25C6) ' ◆ black diamond (reports folder)
                Case TypeOf value Is DetailedReportViewModel,
                     TypeOf value Is ReportViewModel,
                     TypeOf value Is SummaryReportViewModel
                    Return ChrW(&H25CB) ' ○ white circle (report)
                Case TypeOf value Is MaterialCategoryViewModel
                    Return ChrW(&H25C6) ' ◆ black diamond (material category)
                Case TypeOf value Is MaterialViewModel
                    Return ChrW(&H2022) ' • bullet (material item)
                Case Else
                    Return ""
            End Select
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Throw New NotImplementedException()
        End Function

    End Class

End Namespace
