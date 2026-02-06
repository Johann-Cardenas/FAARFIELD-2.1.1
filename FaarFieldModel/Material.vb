Imports System.ComponentModel
Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Runtime.Serialization
Imports System.Windows.Data
'Imports System.Windows.Forms
Imports System.Windows
Imports FaarFieldModel.Interfaces
<KnownType(GetType(AnalysisType))>
<KnownType(GetType(Material))>
Public Class Material
    Implements IMaterial
    Implements INotifyPropertyChanged

    Public Property designoptions As DesignOptions

    Public Property measurementSystem As IMeasurmentSystem

    Dim _factory As IFaarFieldModelFactory
    Public Property _Rupture As Modulus
    ''' <summary>
    ''' This is the default constructor for the Material class.
    ''' </summary>
    Public Sub New()

    End Sub

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    ' This method is called by the Set accessor of each property.
    ' The CallerMemberName attribute that is applied to the optional propertyName
    ' parameter causes the property name of the caller to be substituted as an argument.
    Private Sub OnPropertyChanged(<CallerMemberName()> Optional ByVal info As String = Nothing)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(info))
    End Sub

    Public Sub New(factory As IFaarFieldModelFactory, existing As IMaterial, canDelete As Boolean)
        _factory = factory

        Name = New String(existing.Name)
        Note = New String(existing.Note)
        IsUserDefined = existing.IsUserDefined

        Thickness = factory.CreateThickness(existing.Thickness.UsCustomary, factory.CreateUsCustomary())

        _Modulus = factory.CreateModulus(existing.Modulus.UsCustomary, factory.CreateUsCustomary())
        Modulus = factory.CreateModulus(existing.Modulus.UsCustomary, factory.CreateUsCustomary())

        If Not existing.Rupture Is Nothing Then
            _Rupture = factory.CreateModulus(existing.Rupture.UsCustomary, factory.CreateUsCustomary())
            Rupture = factory.CreateModulus(existing.Rupture.UsCustomary, factory.CreateUsCustomary())
            'Rupture = factory.CreateModulus(existing.Rupture.UsCustomary, factory.CreateUsCustomary())
        End If
        Cbr = existing.Cbr
        If existing.SubgradeReaction IsNot Nothing Then
            SubgradeReaction = factory.CreateSubgradeReaction(existing.SubgradeReaction.UsCustomary, factory.CreateUsCustomary())
        End If
        Me.CanDelete = canDelete
        LayerCode = existing.LayerCode
        CBRActive = existing.CBRActive
        KValueActive = existing.KValueActive
        ThicknessActive = existing.ThicknessActive
        ModulusActive = existing.ModulusActive
        RuptureActive = existing.RuptureActive

        Category = New String(existing.Category)
    End Sub

    Private _CBRActive As Boolean
    Public Property CBRActive As Boolean Implements IMaterial.CBRActive
        Get
            Return _CBRActive
        End Get
        Set(value As Boolean)
            _CBRActive = value
            OnPropertyChanged(NameOf(CBRActive))
        End Set
    End Property

    Private _KValueActive As Boolean
    Public Property KValueActive As Boolean Implements IMaterial.KValueActive
        Get
            Return _KValueActive
        End Get
        Set(value As Boolean)
            _KValueActive = value

            OnPropertyChanged(NameOf(KValueActive))
        End Set
    End Property
    Private _PCAConversionActive As Boolean
    Public Property PCAConversionActive As Boolean Implements IMaterial.PCAConversionActive
        Get
            Return _PCAConversionActive
        End Get
        Set(value As Boolean)
            _PCAConversionActive = value
            If _PCAConversionActive = True Then

                _Modulus = _factory.CreateModulus(((_SubgradeReaction.UsCustomary / 0.8155) ^ (1 / 0.5719)), New UsCustomary)
                '_SubgradeReaction = _factory.CreateSubgradeReaction(0.8155 * (_Modulus.UsCustomary ^ 0.5719), New UsCustomary)
                '_Modulus = _factory.CreateModulus(Math.Pow(_SubgradeReaction.UsCustomary, 1.28405) * 20.15, New UsCustomary)

                OnPropertyChanged(NameOf(SubgradeReaction))
                OnPropertyChanged(NameOf(Modulus))
            ElseIf _NCHRPActive = True Then
                _Modulus = _factory.CreateModulus((2555 * (_Cbr ^ 0.64)), New UsCustomary)
                '_SubgradeReaction = _factory.CreateSubgradeReaction(0.8155 * (_Modulus.UsCustomary ^ 0.5719), New UsCustomary)
                '_Modulus = _factory.CreateModulus(Math.Pow(_SubgradeReaction.UsCustomary, 1.28405) * 20.15, New UsCustomary)

                OnPropertyChanged(NameOf(SubgradeReaction))
                OnPropertyChanged(NameOf(Modulus))
            Else
                If Cbr <> 0 Then
                    _Modulus = _factory.CreateModulus(Math.Pow(_SubgradeReaction.UsCustomary, 1.28405) * 20.15, New UsCustomary)
                    '_Modulus = _factory.CreateModulus((_Cbr * 1500), New UsCustomary)
                    '_SubgradeReaction = _factory.CreateSubgradeReaction(Math.Pow(_Modulus.UsCustomary / 20.15, 1 / 1.28405), New UsCustomary)

                    OnPropertyChanged(NameOf(SubgradeReaction))
                    OnPropertyChanged(NameOf(Modulus))
                End If
            End If

            OnPropertyChanged(NameOf(PCAConversionActive))
        End Set
    End Property
    Private _NCHRPActive As Boolean
    Public Property NCHRPActive As Boolean Implements IMaterial.NCHRPActive
        Get
            Return _NCHRPActive
        End Get
        Set(value As Boolean)
            _NCHRPActive = value
            If _NCHRPActive = True Then

                _Modulus = _factory.CreateModulus((2555 * (_Cbr ^ 0.64)), New UsCustomary)
                '_Modulus = _factory.CreateModulus((2555 * (_Cbr ^ 0.64)), New UsCustomary)
                '_SubgradeReaction = _factory.CreateSubgradeReaction(Math.Pow(_Modulus.UsCustomary / 20.15, 1 / 1.28405), New UsCustomary)
                '_Modulus = _factory.CreateModulus(Math.Pow(_SubgradeReaction.UsCustomary, 1.28405) * 20.15, New UsCustomary))

                'OnPropertyChanged(NameOf(SubgradeReaction))
                OnPropertyChanged(NameOf(Modulus))
                OnPropertyChanged(NameOf(Cbr))
            ElseIf _PCAConversionActive = True Then
                _Modulus = _factory.CreateModulus((_Cbr * 1500), New UsCustomary)
                '_SubgradeReaction = _factory.CreateSubgradeReaction(0.8155 * (_Modulus.UsCustomary ^ 0.5719), New UsCustomary)
                '_Modulus = _factory.CreateModulus(Math.Pow(_SubgradeReaction.UsCustomary, 1.28405) * 20.15, New UsCustomary)

                ' OnPropertyChanged(NameOf(SubgradeReaction))
                OnPropertyChanged(NameOf(Modulus))
                OnPropertyChanged(NameOf(Cbr))

            Else
                If Cbr <> 0 Then
                    _Modulus = _factory.CreateModulus((_Cbr * 1500), New UsCustomary)
                    '_Modulus = _factory.CreateModulus((_Cbr * 1500), New UsCustomary)
                    _SubgradeReaction = _factory.CreateSubgradeReaction(Math.Pow(_Modulus.UsCustomary / 20.15, 1 / 1.28405), New UsCustomary)


                End If
                OnPropertyChanged(NameOf(SubgradeReaction))
                OnPropertyChanged(NameOf(Modulus))
                OnPropertyChanged(NameOf(Cbr))
            End If

            OnPropertyChanged(NameOf(NCHRPActive))
        End Set
    End Property
    Private _ModulusActive As Boolean
    Public Property ModulusActive As Boolean Implements IMaterial.ModulusActive
        Get
            Return _ModulusActive
        End Get
        Set(value As Boolean)
            _ModulusActive = value
            OnPropertyChanged(NameOf(ModulusActive))
        End Set
    End Property

    Private _RuptureActive As Boolean
    Public Property RuptureActive As Boolean Implements IMaterial.RuptureActive
        Get
            Return _RuptureActive
        End Get
        Set(value As Boolean)
            _RuptureActive = value
            OnPropertyChanged(NameOf(RuptureActive))
        End Set
    End Property

    Private _ThicknessActive As Boolean
    Public Property ThicknessActive As Boolean Implements IMaterial.ThicknessActive
        Get
            Return _ThicknessActive
        End Get
        Set(value As Boolean)
            _ThicknessActive = value
            OnPropertyChanged(NameOf(ThicknessActive))
        End Set
    End Property

    Private _Category As String
    Public Property Category As String Implements IMaterial.Category
        Get
            Return _Category
        End Get
        Set(value As String)
            _Category = value
            OnPropertyChanged(NameOf(Category))
        End Set
    End Property

    Private _Name As String
    Public Property Name As String Implements IMaterial.Name
        Get
            Return _Name
        End Get
        Set(value As String)
            _Name = value
            OnPropertyChanged(NameOf(Name))
        End Set
    End Property

    Private _DesignedLayer As String
    Public Property DesignedLayer As String Implements IMaterial.DesignedLayer
        Get
            Return _DesignedLayer
        End Get
        Set(value As String)
            _DesignedLayer = value
            OnPropertyChanged(NameOf(DesignedLayer))
        End Set
    End Property

    Private _Note As String
    Public Property Note As String Implements IMaterial.Note
        Get
            Return _Note
        End Get
        Set(value As String)
            _Note = value
            OnPropertyChanged(NameOf(Note))
        End Set
    End Property

    Private _IsUserDefined As Boolean
    Public Property IsUserDefined As Boolean Implements IMaterial.IsUserDefined
        Get
            Return _IsUserDefined
        End Get
        Set(value As Boolean)
            _IsUserDefined = value
            OnPropertyChanged(NameOf(IsUserDefined))
        End Set
    End Property

    Private _CanDelete As Boolean
    Public Property CanDelete As Boolean Implements IMaterial.CanDelete
        Get
            Return _CanDelete
        End Get
        Set(value As Boolean)
            _CanDelete = value
            OnPropertyChanged(NameOf(CanDelete))
        End Set
    End Property

    Private _LayerCode As Integer
    Public Property LayerCode As Integer Implements IMaterial.LayerCode
        Get
            Return _LayerCode
        End Get
        Set(value As Integer)
            _LayerCode = value
            OnPropertyChanged(NameOf(LayerCode))
        End Set
    End Property

    Private _OnCheckedDesignLayer As Boolean
    Public Property OnCheckedDesignLayer As Boolean Implements IMaterial.OnCheckedDesignLayer
        Get
            Return _OnCheckedDesignLayer
        End Get
        Set(value As Boolean)
            _OnCheckedDesignLayer = value
            OnPropertyChanged(NameOf(OnCheckedDesignLayer))
        End Set
    End Property


    Public Property Rupture As Modulus Implements IMaterial.Rupture
        Get
            Return _Rupture
        End Get
        Set(value As Modulus)

            If Not TypeOf measurementSystem Is Metric Then
                If Category = "P-501 PCC" Then
                    If 500 <= Convert.ToSingle(value.UsCustomary) And Convert.ToSingle(value.UsCustomary) <= 1000 Then
                        _Rupture = value
                    Else
                        MessageBox.Show("PCC flexural strength(R) can be set in the range 500 to 1000 psi (3.45 to 6.9 MPa). The value entered is outside the allowed range. " & vbNewLine &
                                        "The default value of 650 psi (4.48 MPa) will be selected.")

                        _Rupture = _factory.CreateModulus(650, New UsCustomary)
                    End If
                End If

            End If

            OnPropertyChanged(NameOf(Rupture))
        End Set
    End Property





    'Dim _thickness As Thickness
    'Dim _modulus As Modulus
    'Dim _cbr As Double
    'Dim _subgradeReaction As SubgradeReaction

    'Public Property Note As String Implements IMaterial.Note
    'Public Property IsUserDefined As Boolean Implements IMaterial.IsUserDefined
    'Public Property CanDelete As Boolean Implements IMaterial.CanDelete
    'Public Property LayerCode As Integer Implements IMaterial.LayerCode
    'Public Property _OnCheckedDesignLayer As Boolean Implements IMaterial.OnCheckedDesignLayer
    'Public Property _Rupture As Modulus

    Public Property _SubgradeReaction As SubgradeReaction
    Public Property SubgradeReaction As SubgradeReaction Implements IMaterial.SubgradeReaction
        Get
            If KValueActive Then
                Return _SubgradeReaction
            Else
                Return Nothing
            End If
        End Get
        Set(value As SubgradeReaction)
            _SubgradeReaction = value
            If value IsNot Nothing Then
                If Name = "Subgrade" Or Name = "New Rigid" Or Name = "HMA Overlay on Rigid" Or Name = "PCC Overlay on Flexible" Or Name = "Unbonded PCC Overlay on Rigid" Then
                    If _SubgradeReaction.UsCustomary < 20.9 Or _SubgradeReaction.UsCustomary > 440.4 Then
                        Dim msg As String = "Subgrade layer k value can be set between " + vbNewLine
                        msg += "20.9 to 440.4 pci (5.7 to 119.5 MN/m^3)." + vbNewLine
                        MessageBox.Show(msg)
                        _SubgradeReaction = _factory.CreateSubgradeReaction(172.4, New UsCustomary)

                    End If


                End If
            End If


            If Name = "Subgrade" Or Name = "User Defined" Then

                If Not value Is Nothing Then

                    If _factory IsNot Nothing Then
                        If _PCAConversionActive = True Then
                            _Modulus = _factory.CreateModulus(((_SubgradeReaction.UsCustomary / 0.8155) ^ (1 / 0.5719)), New UsCustomary)
                            ' _Cbr = _Modulus.UsCustomary / 1500
                            OnPropertyChanged(NameOf(Modulus))
                            'OnPropertyChanged(NameOf(Cbr))

                        ElseIf _NCHRPActive = True Then
                            '_Cbr = (26 / 1500) * (_SubgradeReaction.UsCustomary ^ (1 / 0.7788))
                            '_Modulus = _factory.CreateModulus(_Cbr * 1500, New UsCustomary)
                            _Modulus = _factory.CreateModulus((2555 * (_Cbr ^ 0.64)), New UsCustomary)
                            OnPropertyChanged(NameOf(Modulus))
                            'OnPropertyChanged(NameOf(Cbr))
                        Else
                            If Cbr <> 0 Then
                                _Modulus = _factory.CreateModulus(Math.Pow(_SubgradeReaction.UsCustomary, 1.28405) * 20.15, New UsCustomary)
                                _Cbr = _Modulus.UsCustomary / 1500
                                OnPropertyChanged(NameOf(Modulus))
                                OnPropertyChanged(NameOf(Cbr))
                            End If
                        End If
                    End If
                End If


            End If

            OnPropertyChanged(NameOf(Modulus))
            OnPropertyChanged(NameOf(SubgradeReaction))
            OnPropertyChanged(NameOf(Cbr))
        End Set
    End Property

    Private _Modulus As Modulus
    Public Property Modulus As Modulus Implements IMaterial.Modulus
        Get
            Return _Modulus
        End Get
        Set(value As Modulus)
            'TODO: Factory is not set when creating Subgrade Material
            '      This is quick temp fix
            If _factory Is Nothing Then
                _factory = New FaarFieldModelFactory
                Debug.WriteLine(":: _factory is Nothing :: FaarFieldModelFactory.Material.Modulus")
            End If
            If Name = "User Defined" Then
                If value.UsCustomary < 1000 Or value.UsCustomary > 4000000 Then
                    Dim msg = "User Defined modulus values can be set in the range 1,000 psi to 4,000,000 psi (6.89 to 27579.04 MPa)."
                    MessageBox.Show(msg)
                    _Modulus = _factory.CreateModulus(100000, New UsCustomary)
                Else

                    _Modulus = value
                End If
            ElseIf Name = "Variable (flexible)" Then
                If value.UsCustomary < 150000 Or value.UsCustomary > 400000 Then
                    Dim msg = "Variable (Flexible) modulus values can be set in the range 150,000 psi to 400,000 psi (1034.21 to 2757.90 MPa)."
                    MessageBox.Show(msg)
                    _Modulus = _factory.CreateModulus(150000, New UsCustomary)
                Else
                    _Modulus = value
                End If

            ElseIf Name = "Variable (rigid)" Then
                If value.UsCustomary < 250000 Or value.UsCustomary > 700000 Then

                    Dim msg = "Variable (Rigid) modulus values can be set in the range 250,000 psi to 700,000 psi (1723.69 MPa - 4826.33 MPa)."
                    MessageBox.Show(msg)
                    _Modulus = _factory.CreateModulus(250000, New UsCustomary)
                Else
                    _Modulus = value
                End If
            ElseIf Name = "Subgrade" Then
                If value.UsCustomary < 1000 Or value.UsCustomary > 50000 Then

                    Dim msg = "Subgrade modulus values can be set in the range 1,000 psi to 50,000 psi (6.89 MPa - 344.74 MPA)."
                    MessageBox.Show(msg)
                    _Modulus = _factory.CreateModulus(15000, New UsCustomary)

                Else
                    _Modulus = value
                End If
            Else
                _Modulus = value
            End If
            If Name = "Subgrade" Or Name = "User Defined" Then
                If Not value Is Nothing Then
                    If _PCAConversionActive = True Then
                        _SubgradeReaction = _factory.CreateSubgradeReaction(0.8155 * (_Modulus.UsCustomary ^ 0.5719), New UsCustomary)
                        '_Modulus = _factory.CreateModulus(Math.Pow(_SubgradeReaction.UsCustomary, 1.28405) * 20.15, New UsCustomary)
                        OnPropertyChanged(NameOf(SubgradeReaction))
                    ElseIf _NCHRPActive = True Then
                        _Cbr = (Modulus.UsCustomary / 2555) ^ (1 / 0.64)
                        '_Modulus = _factory.CreateModulus((2555 * (_Cbr ^ 0.64)), New UsCustomary)
                        '_Modulus = _factory.CreateModulus(Math.Pow(_SubgradeReaction.UsCustomary, 1.28405) * 20.15, New UsCustomary)
                        OnPropertyChanged(NameOf(Cbr))
                    Else
                        _Cbr = _Modulus.UsCustomary / 1500
                        _SubgradeReaction = _factory.CreateSubgradeReaction(Math.Pow(_Modulus.UsCustomary / 20.15, 1 / 1.28405), New UsCustomary)
                        OnPropertyChanged(NameOf(Cbr))
                        OnPropertyChanged(NameOf(SubgradeReaction))
                    End If
                    OnPropertyChanged(NameOf(Cbr))
                    OnPropertyChanged(NameOf(SubgradeReaction))

                End If

            End If

            OnPropertyChanged(NameOf(Modulus))
        End Set
    End Property


    Private Property _Cbr As Double
    Public Property Cbr As Double Implements IMaterial.Cbr
        Get
            If CBRActive Then
                Return _Cbr
            Else
                Return Nothing
            End If
        End Get
        Set(value As Double)
            _Cbr = value

            If Name = "Subgrade" Then

                If (0 < _Cbr And _Cbr < 0.7) Or _Cbr > 33.3 Then
                    Dim msg As String = "Subgrade layer CBR value can be set in the range 0.7 to 33.3 percent. "

                    MessageBox.Show(msg)
                    _Cbr = 10


                End If
                If _Cbr <> 0 Then
                    If NCHRPActive = True Then
                        _Modulus = _factory.CreateModulus((2555 * (_Cbr ^ 0.64)), New UsCustomary)
                        '_SubgradeReaction = _factory.CreateSubgradeReaction(Math.Pow(_Modulus.UsCustomary / 20.15, 1 / 1.28405), New UsCustomary)
                    Else
                        _Modulus = _factory.CreateModulus(_Cbr * 1500, New UsCustomary)
                        _SubgradeReaction = _factory.CreateSubgradeReaction(Math.Pow(_Modulus.UsCustomary / 20.15, 1 / 1.28405), New UsCustomary)
                    End If
                    OnPropertyChanged(NameOf(Modulus))
                    OnPropertyChanged(NameOf(SubgradeReaction))
                    OnPropertyChanged(NameOf(Cbr))
                End If
            End If

            If Name = "User Defined" Then

                If _Cbr > 2666.7 Then
                    Dim msg As String = "User Defined layer CBR value can be set in the range 0.7 to 2,666.7 percent. "
                    MessageBox.Show(msg)

                    _Cbr = 66.6666

                End If

                '_Modulus = _factory.CreateModulus(_Cbr * 1500, New UsCustomary)
                '_SubgradeReaction = _factory.CreateSubgradeReaction(Math.Pow(_Modulus.UsCustomary / 20.15, 1 / 1.28405), New UsCustomary)
                If NCHRPActive = True Then
                    _Modulus = _factory.CreateModulus((2555 * (_Cbr ^ 0.64)), New UsCustomary)
                    '_SubgradeReaction = _factory.CreateSubgradeReaction(Math.Pow(_Modulus.UsCustomary / 20.15, 1 / 1.28405), New UsCustomary)

                ElseIf _Cbr <> 0 Then
                    _Modulus = _factory.CreateModulus(_Cbr * 1500, New UsCustomary)
                    _SubgradeReaction = _factory.CreateSubgradeReaction(Math.Pow(_Modulus.UsCustomary / 20.15, 1 / 1.28405), New UsCustomary)
                End If

                OnPropertyChanged(NameOf(Modulus))
                OnPropertyChanged(NameOf(SubgradeReaction))

            End If

            OnPropertyChanged(NameOf(Cbr))

        End Set
    End Property


    Private Property _Thickness As Thickness
    Public Property Thickness As Thickness Implements IMaterial.Thickness
        Get
            If ThicknessActive Then
                Return _Thickness
            Else
                Return _factory.CreateThickness(6, _factory.CreateUsCustomary())
            End If

        End Get
        Set(value As Thickness)
            _Thickness = value

            'If Name = "P-208 Crushed Aggregate" Then
            '    If value.UsCustomary < 6 Then
            '        MessageBox.Show("The thickness of P-208 Crushed Aggregate can not be less than 6 inches (152.4 mm).")

            '        value = _factory.CreateThickness(6, _factory.CreateUsCustomary())
            '        _Thickness = value

            '    End If
            'End If
            OnPropertyChanged(NameOf(Thickness))
        End Set
    End Property

    Private Property _ButtonHeight As Integer
    Public Property ButtonHeight As Integer Implements IMaterial.ButtonHeight
        Get
            Return _ButtonHeight
        End Get
        Set(value As Integer)
            _ButtonHeight = value
            OnPropertyChanged(NameOf(ButtonHeight))
        End Set
    End Property

End Class
