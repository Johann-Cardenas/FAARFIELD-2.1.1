Imports System.IO
Imports System.Runtime.Serialization
Imports System.Runtime.Serialization.Json
Imports System.Text
Imports FaarFieldModel.Interfaces
Imports Microsoft.VisualBasic.FileIO
<DataContract>
<KnownType(GetType(FaarFieldJob))>
<KnownType(GetType(Section))>
<KnownType(GetType(DesignOptions))>
<KnownType(GetType(JobInformation))>
Public Class FaarFieldJob
    Implements IFaarFieldJob
    Implements IModelAction

    Private m_Name As String = ""

    <DataMember>
    Public Property Name As String Implements IFaarFieldJob.Name
        Get
            Return m_Name
        End Get
        Set(value As String)
            If Not value.Trim() = "" Then
                m_Name = value
            End If
        End Set
    End Property

    <DataMember>
    Public Property JobInformation As IJobInformation Implements IFaarFieldJob.JobInformation

    <DataMember>
    Public Property Sections As List(Of ISection) Implements IFaarFieldJob.Sections

    <DataMember>
    Public Property DesignOptions As IDesignOptions Implements IFaarFieldJob.DesignOptions


    Private _Version_1_4_File As Boolean = False
    Public Property Version_1_4_File As Boolean Implements IFaarFieldJob.Version_1_4_File
        Get
            Return _Version_1_4_File
        End Get
        Set(value As Boolean)
            _Version_1_4_File = value
        End Set
    End Property


    ''' <summary>
    ''' This is the default FaarFieldJob constructor.
    ''' </summary>
    Public Sub New()

    End Sub

    ''' <summary>
    ''' This is one of the constructors for the FaarFieldJob class.
    ''' </summary>
    ''' <param name="factory">The factory that is used to create a faarfield job.</param>
    ''' <param name="defaultFilePath">A string variable that is the default file path of the job created.</param>
    Sub New(factory As IFaarFieldModelFactory, defaultFilePath As String)
        JobInformation = factory.CreateJobInformation(factory)

        If File.Exists(SpecialDirectories.MyDocuments + "\My FAARFIELD\UserDefaults\CustomDesignOptions.json") Then
            Dim loadPath = SpecialDirectories.MyDocuments + "\My FAARFIELD\UserDefaults\CustomDesignOptions.json"
            Dim ser = New DataContractJsonSerializer(GetType(DesignOptions))
            Dim textReader As TextReader = New System.IO.StreamReader(loadPath.ToString())
            Dim json = textReader.ReadToEnd()
            Dim stream = New MemoryStream(Encoding.Unicode.GetBytes(json))
            Dim customDesignOptions = DirectCast(ser.ReadObject(stream), DesignOptions)
            DesignOptions = New DesignOptions(factory, customDesignOptions)
            textReader.Close()
        Else
            DesignOptions = New DesignOptions(factory)
        End If
        Sections = New List(Of ISection)
    End Sub

    ''' <summary>
    ''' This is the constructor for the FaarFieldJob class if there is an existing job to clone.
    ''' </summary>
    ''' <param name="factory">The factory that is used to create a faarfield job.</param>
    ''' <param name="existing">This is an already existing job that gets its value copied.</param>
    Sub New(factory As IFaarFieldModelFactory, existing As IFaarFieldJob)
        JobInformation = factory.CreateJobInformation(factory, existing.JobInformation)
        DesignOptions = factory.CreateDesignOptions(factory, existing.DesignOptions)
        Sections = New List(Of ISection)
        Name = existing.Name

        For Each section In existing.Sections
            Sections.Add(factory.CreateSection(factory, section))
        Next
    End Sub

    Public Function Validate(measurementSystem As IMeasurmentSystem) As List(Of IValidation) Implements IModelAction.Validate
        Throw New NotImplementedException
    End Function
End Class
