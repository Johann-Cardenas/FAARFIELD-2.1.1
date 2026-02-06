Option Strict On
Option Explicit On
Module INITLIBS
    Public RunAcLib As New ACClassLib.clsAC()
    Public AC() As ACClassLib.clsAC.AircraftCharacteristics

    Sub InitAcLib()
        RunAcLib.TrafficList = FEDFAA1.TrafficList
        Call RunAcLib.InitACLib(AC, LibACGroup, LibACGroupName,
                                libNAC, NBelly, NLibACGroups, InitTraffic)
    End Sub


    Sub InitLayers()

        ' Global Const NLayerTypes%.   Set in Global declarations.
        ' LayerType$ strings are also set in Global declarations as Consts.
        ' These strings are used in control statements like:
        ' If LayerType$(LCode(I)) = S$ Then DoSomething. Therefore,
        ' if new layer types are added, also add associated Consts
        ' in Global declarations. This is done so that descriptions or
        ' LCode numbers can be changed easily.

        ' If LCode values are changed then file input and samples
        ' init. routines must be changed.

        ' LayerTypeEx$ strings are included for printing complete
        ' information when necessary. They should not be used in
        ' control statements.

        ' Julea ANDs code index values 1 and 2 for conditional
        ' branching. Code index 2 is therefore not used, and 3 has also
        ' been dropped to be consistent. The use of codes 2 and 3 is
        ' disallowed in lstStrFiles. Their option buttons have been hidden
        ' in dlgLayerType. If JULEA is changed then the unused option
        ' buttons can be put back into service.
        ' Not applicable with Leaf, GFH 04/24/03.

        ' CHANGES FOR V 1.1.
        ' Four new layer types added as:
        ' P-401 stabilized (flexible)
        ' P-301 SCTB stabilized (rigid)
        ' P-304 CTB stabilized (rigid)
        ' P-306 Econocrete stabilized (rigid)
        ' The list was also changed so that types are grouped. The indexes
        ' were not changed in case they had been referred to explicitly
        ' in the program. The order of types was changed below to agree
        ' with the new order in the list. Indexes are therefore not in order.
        ' The new types are given the old LayerType$() strings for
        ' stabilized bases. Design selection should therefore be correct.

        ' For reference:
        ' LayerTypeEx$(0)  = "User Defined"
        ' LayerTypeEx$(1)  = "P-401 Asphalt Surface"
        ' LayerTypeEx$(2)  = "Base"       For compatibility with JINDAT. ***
        ' LayerTypeEx$(3)  = "Subbase"    For compatibility with JINDAT. ***
        ' LayerTypeEx$(4)  = "Subgrade"
        ' LayerTypeEx$(5)  = "P-501 PCC Surface"
        ' LayerTypeEx$(6)  = "P-209 Crushed Aggregate Base Course"
        ' LayerTypeEx$(7)  = "Variable Stabilized Base (rigid)"
        ' LayerTypeEx$(8)  = "P-154 Subbase Course (uncrushed aggregate)"
        ' LayerTypeEx$(9)  = "Variable Stabilized Base (flexible)"
        ' LayerTypeEx$(10) = "P-401 Asphalt Overlay"
        ' LayerTypeEx$(11) = "P-501 PCC overlay fully unbonded"
        ' LayerTypeEx$(12) = "P-501 PCC overlay partially bonded"
        ' LayerTypeEx$(13) = "P-501 PCC overlay on flexible"
        ' LayerTypeEx$(14) = "P-401 Stabilized Base (flexible)"
        ' LayerTypeEx$(15) = "P-301 Soil Cement Base Course"
        ' LayerTypeEx$(16) = "P-304 Cement Treated Base Course"
        ' LayerTypeEx$(17) = "P-306 Econocrete Subbase Course"
        ' LayerTypeEx$(18) = "P-208 Crushed Aggregate Base Course"
        ' LayerTypeEx$(19) = "Rubblized PCC"
        ' LayerTypeEx$(20) = "P-401 Stabilized Base (flexible)" as 14
        ' LayerTypeEx$(21) = "P-211" 



        Dim MrOfPCC As Single = 650


        LayerType$(0) = NND$              ' Used for selection of design procedure.
        LayerTypeEx$(0) = "User Defined"     ' Used for printing full information.
        LayerTypeList$(0) = "User Defined"   ' Used in type change dialog box.
        DefaultPoisson(0) = 0.35
        ThickMin(0) = 2.0!

        LayerType$(1) = NAsp$              ' For compatibility with JINDAT.
        LayerTypeEx$(1) = "P-401/ P-403 Asphalt Surface"
        LayerTypePic$(1) = "P-401/ P-403 HMA Surface"
        LayerTypeList$(1) = "Surface"
        DefaultPoisson(1) = 0.35
        DefaultModulus(1) = 200000.0!
        DefaultInterface(1) = 0            ' Fully bonded.
        ThickMin(1) = 4.0!
        '  ThickMin(1) = 2!
        DefaultSCIB(1) = 0.0! : SCIBMin(1) = 0.0! : SCIBMax(1) = 0.0!
        DefaultLifeExistPCC(1) = 0.0! : LifeExistPCCMin(1) = 0.0! : LifeExistPCCMax(1) = 0.0!

        LayerType$(2) = NBase$             ' For compatibility with JINDAT.
        LayerTypeEx$(2) = "Base"           ' See note above.
        DefaultPoisson(2) = 0.35
        DefaultModulus(2) = 75000.0!
        DefaultInterface(2) = 0            ' Fully bonded.
        ThickMin(2) = 4.0!
        DefaultSCIB(2) = 0.0! : SCIBMin(2) = 0.0! : SCIBMax(2) = 0.0!
        DefaultLifeExistPCC(2) = 0.0! : LifeExistPCCMin(2) = 0.0! : LifeExistPCCMax(2) = 0.0!

        LayerType$(3) = NSBase$            ' For compatibility with JINDAT.
        LayerTypeEx$(3) = "Subbase"        ' See note above.
        DefaultPoisson(3) = 0.35
        DefaultModulus(3) = 40000.0!
        DefaultInterface(3) = 0            ' Fully bonded.
        ThickMin(3) = 4.0!
        DefaultSCIB(3) = 0.0! : SCIBMin(3) = 0.0! : SCIBMax(3) = 0.0!
        DefaultLifeExistPCC(3) = 0.0! : LifeExistPCCMin(3) = 0.0! : LifeExistPCCMax(3) = 0.0!

        LayerType$(4) = NSG$              ' For compatibility with JINDAT.
        LayerTypeEx$(4) = "Subgrade"
        LayerTypePic$(4) = "Subgrade"
        LayerTypeList$(4) = "Subgrade"
        DefaultPoisson(4) = 0.35
        DefaultPoissonSGAC = 0.35
        DefaultPoissonSGPCC = 0.4
        DefaultModulus(4) = 15000.0!
        DefaultInterface(4) = 0            ' Fully bonded.
        ThickMin(4) = 5.0!
        DefaultSCIB(4) = 0.0! : SCIBMin(4) = 0.0! : SCIBMax(4) = 0.0!
        DefaultLifeExistPCC(4) = 0.0! : LifeExistPCCMin(4) = 0.0! : LifeExistPCCMax(4) = 0.0!

        LayerType$(5) = NPCC$
        LayerTypeEx$(5) = "P-501 PCC Surface"
        LayerTypePic$(5) = "PCC Surface"
        LayerTypeList$(5) = "Surface"
        DefaultPoisson(5) = 0.15
        DefaultInterface(5) = 100000        ' Fully unbonded.
        DefaultRCON = MrOfPCC 'was 700
        DefaultModulus(5) = 4000000.0!
        '  DefaultModulus(5) = 1000000!
        DefaultInterface(5) = 100000        ' Fully unbonded.
        ThickMin(5) = 4.0!
        RCONMin = 500.0! : RCONMax = 900.0! 'modified by YGC 091813 

        LayerType$(6) = NAgBase$
        LayerTypeEx$(6) = "P-209 Crushed Aggregate Base Course"
        LayerTypePic$(6) = "P-209 Cr Ag"
        LayerTypeList$(6) = "P-209 Crushed"
        DefaultPoisson(6) = 0.35
        DefaultModulus(6) = 75000.0!
        DefaultInterface(6) = 0             ' Fully bonded.
        ThickMin(6) = 4.0!
        DefaultSCIB(6) = 0.0! : SCIBMin(6) = 0.0! : SCIBMax(6) = 0.0!
        DefaultLifeExistPCC(6) = 0.0! : LifeExistPCCMin(6) = 0.0! : LifeExistPCCMax(6) = 0.0!

        LayerType$(7) = NStBase$
        LayerTypeEx$(7) = "Variable Stabilized Base (rigid)"
        LayerTypePic$(7) = "Variable St (rigid)"
        LayerTypeList$(7) = "Variable"
        DefaultPoisson(7) = 0.2
        DefaultPoisson(7) = 0.2
        DefaultModulus(7) = 250000.0!
        DefaultInterface(7) = 0             ' Fully bonded.
        ThickMin(7) = 4.0!
        DefaultSCIB(7) = 0.0! : SCIBMin(7) = 0.0! : SCIBMax(7) = 0.0!
        DefaultLifeExistPCC(7) = 0.0! : LifeExistPCCMin(7) = 0.0! : LifeExistPCCMax(7) = 0.0!

        LayerType$(8) = NAgSBase$
        LayerTypeEx$(8) = "P-154 Subbase Course (uncrushed aggregate)"
        LayerTypePic$(8) = "P-154 UnCr Ag"
        LayerTypeList$(8) = "P-154 Uncrushed"
        DefaultPoisson(8) = 0.35
        DefaultModulus(8) = 40000.0!
        'ModulusMin(8) = 1000.0! : ModulusMax(8) = 8000000.0!
        DefaultInterface(8) = 0             ' Fully bonded.
        ThickMin(8) = 4.0!
        '  ThickMin(8) = 1!
        DefaultSCIB(8) = 0.0! : SCIBMin(8) = 0.0! : SCIBMax(8) = 0.0!
        DefaultLifeExistPCC(8) = 0.0! : LifeExistPCCMin(8) = 0.0! : LifeExistPCCMax(8) = 0.0!

        LayerType$(9) = NStSBase$
        LayerTypeEx$(9) = "Variable Stabilized Base (flexible)"
        LayerTypePic$(9) = "Variable St (flex)"
        LayerTypeList$(9) = "Variable"
        DefaultPoisson(9) = 0.35
        DefaultModulus(9) = 150000.0!
        ' ModulusMin(9) = 150000.0! : ModulusMax(9) = 400000.0!
        DefaultInterface(9) = 0              ' Fully bonded.
        ThickMin(9) = 5.0!
        DefaultSCIB(9) = 0.0! : SCIBMin(9) = 0.0! : SCIBMax(9) = 0.0!
        DefaultLifeExistPCC(9) = 0.0! : LifeExistPCCMin(9) = 0.0! : LifeExistPCCMax(9) = 0.0!

        LayerType$(10) = NACO$
        LayerTypeEx$(10) = "P-401/ P-403 Asphalt Overlay"
        LayerTypePic$(10) = "P-401/ P-403 HMA Overlay"
        LayerTypeList$(10) = "Overlay"
        DefaultPoisson(10) = 0.35
        DefaultLifeExistPCC(10) = 100.0!
        DefaultModulus(10) = 200000.0!
        'ModulusMin(10) = 50000.0! : ModulusMax(10) = 200000.0!
        DefaultInterface(10) = 0             ' Fully bonded.
        ThickMin(10) = 2.0!
        DefaultSCIB(10) = 50.0! : SCIBMin(10) = 50.0! : SCIBMax(10) = 100.0!
        'DefaultSCIB(10) = 67.0! : SCIBMin(10) = 67.0! 'ikawa June 27, 2005
        DefaultSCIB(10) = 80.0! : SCIBMin(10) = 67.0! 'ikawa Apil 28, 2005
        DefaultLifeExistPCC(10) = 100.0!
        LifeExistPCCMin(10) = 10.0!
        LifeExistPCCMax(10) = 100.0!

        LayerType$(11) = NPCCOU$
        LayerTypeEx$(11) = "P-501 PCC overlay fully unbonded"
        LayerTypePic$(11) = "PCC Overlay Unbond"
        LayerTypeList$(11) = "Overlay fully unbonded"
        DefaultPoisson(11) = 0.15
        DefaultInterface(11) = 100000        ' Fully unbonded.
        DefaultLifeExistPCC(11) = 100.0!
        DefaultModulus(11) = DefaultModulus(5)
        'ModulusMin(11) = ModulusMin(5) : ModulusMax(11) = ModulusMax(5)
        DefaultInterface(11) = 100000        ' Fully unbonded.
        ThickMin(11) = 5.0!
        ' See code 5 for concrete strength.
        DefaultSCIB(11) = 80.0! : SCIBMin(11) = 67.0! : SCIBMax(11) = 100.0!
        DefaultLifeExistPCC(11) = 100.0!
        LifeExistPCCMin(11) = 10.0!
        LifeExistPCCMax(11) = 100.0!

        LayerType$(12) = NPCCOB$
        LayerTypeEx$(12) = "P-501 PCC overlay partially bonded"
        LayerTypePic$(12) = "PCC Overlay Part Bond"
        LayerTypeList$(12) = "Overlay partially bonded"
        DefaultPoisson(12) = 0.15
        DefaultInterface(12) = 700           ' Partially bonded.
        DefaultLifeExistPCC(12) = 100.0!
        DefaultModulus(12) = DefaultModulus(5)
        ' ModulusMin(12) = ModulusMin(5) : ModulusMax(12) = ModulusMax(5)
        DefaultInterface(12) = 700           ' Partially bonded.
        ThickMin(12) = 5.0!
        ' See code 5 for concrete strength.
        DefaultSCIB(12) = 80.0! : SCIBMin(12) = 67.0! : SCIBMax(12) = 100.0!
        DefaultLifeExistPCC(12) = 100.0!
        LifeExistPCCMin(12) = 10.0!
        LifeExistPCCMax(12) = 100.0!

        LayerType$(13) = NPCCOF$
        LayerTypeEx$(13) = "P-501 PCC overlay on flexible"
        LayerTypePic$(13) = "PCC Overlay on Flex."
        LayerTypeList$(13) = "Overlay on flexible"
        DefaultPoisson(13) = DefaultPoisson(5)
        DefaultInterface(13) = DefaultInterface(5)
        DefaultPoisson(13) = DefaultPoisson(5)
        DefaultModulus(13) = DefaultModulus(5)
        ' ModulusMin(13) = ModulusMin(5) : ModulusMax(13) = ModulusMax(5)
        DefaultInterface(13) = DefaultInterface(5)
        ThickMin(13) = 5.0!
        ' See code 5 for concrete strength.
        DefaultSCIB(13) = DefaultSCIB(5) : SCIBMin(13) = SCIBMin(5) : SCIBMax(13) = SCIBMax(5)
        DefaultLifeExistPCC(13) = DefaultLifeExistPCC(5)
        LifeExistPCCMin(13) = LifeExistPCCMin(5)
        LifeExistPCCMax(13) = LifeExistPCCMax(5)

        LayerType$(14) = NStSBase$
        LayerTypeEx$(14) = "P-401 Stabilized Base (flexible)"
        LayerTypePic$(14) = "P-401/ P-403 St (flex)"
        LayerTypeList$(14) = "P-401/ P-403 HMA"
        DefaultPoisson(14) = 0.35
        DefaultModulus(14) = 400000.0!
        'ModulusMin(14) = 400000.0! : ModulusMax(14) = 400000.0!
        DefaultInterface(14) = 0              ' Fully bonded.
        ThickMin(14) = 4.0!
        DefaultSCIB(14) = 0.0! : SCIBMin(14) = 0.0! : SCIBMax(14) = 0.0!
        DefaultLifeExistPCC(14) = 0.0! : LifeExistPCCMin(14) = 0.0! : LifeExistPCCMax(14) = 0.0!

        LayerType$(15) = NStBase$
        LayerTypeEx$(15) = "P-301 Soil Cement Base Course"
        LayerTypePic$(15) = "P-301 SCB"
        LayerTypeList$(15) = "P-301 Soil Cement Base" 'SCB"
        DefaultPoisson(15) = 0.2
        DefaultModulus(15) = 250000.0!
        'ModulusMin(15) = 250000.0! : ModulusMax(15) = 250000.0!
        DefaultInterface(15) = 0             ' Fully bonded.
        ThickMin(15) = 4.0!
        DefaultSCIB(15) = 0.0! : SCIBMin(15) = 0.0! : SCIBMax(15) = 0.0!
        DefaultLifeExistPCC(15) = 0.0! : LifeExistPCCMin(15) = 0.0! : LifeExistPCCMax(15) = 0.0!

        LayerType$(16) = NStBase$
        LayerTypeEx$(16) = "P-304 Cement Treated Base Course"
        LayerTypePic$(16) = "P-304 CTB"
        LayerTypeList$(16) = "P-304 Cement Treated Base"
        DefaultPoisson(16) = 0.2
        DefaultModulus(16) = 500000.0!
        'ModulusMin(16) = 500000.0! : ModulusMax(16) = 500000.0!
        DefaultInterface(16) = 0             ' Fully bonded.
        ThickMin(16) = 4.0!
        DefaultSCIB(16) = 0.0! : SCIBMin(16) = 0.0! : SCIBMax(16) = 0.0!
        DefaultLifeExistPCC(16) = 0.0! : LifeExistPCCMin(16) = 0.0! : LifeExistPCCMax(16) = 0.0!

        LayerType$(17) = NStBase$
        LayerTypeEx$(17) = "P-306 Econocrete Subbase Course"
        LayerTypePic$(17) = "P-306 Econocrete"
        LayerTypeList$(17) = "P-306 Econocrete Subbase"
        DefaultPoisson(17) = 0.2
        DefaultModulus(17) = 700000.0!
        'ModulusMin(17) = 700000.0! : ModulusMax(17) = 700000.0!
        DefaultInterface(17) = 0             ' Fully bonded.
        ThickMin(17) = 4.0!
        DefaultSCIB(17) = 0.0! : SCIBMin(17) = 0.0! : SCIBMax(17) = 0.0!
        DefaultLifeExistPCC(17) = 0.0! : LifeExistPCCMin(17) = 0.0! : LifeExistPCCMax(17) = 0.0!

        LayerType$(18) = NAgBase$
        LayerTypeEx$(18) = "P-208 Crushed Aggregate Base Course"
        LayerTypePic$(18) = "P-208 Cr Ag"
        LayerTypeList$(18) = "P-208"
        DefaultPoisson(18) = 0.35
        DefaultModulus(18) = 75000.0!
        'ModulusMin(18) = 1000.0! : ModulusMax(18) = 8000000.0!
        DefaultInterface(18) = 0             ' Fully bonded.
        ThickMin(18) = 5.0! '                  4.0! for P-209
        DefaultSCIB(18) = 0.0! : SCIBMin(18) = 0.0! : SCIBMax(18) = 0.0!
        DefaultLifeExistPCC(18) = 0.0! : LifeExistPCCMin(18) = 0.0! : LifeExistPCCMax(18) = 0.0!

        'LayerType$(19) = NRBase$
        LayerType$(19) = NAgBase$
        LayerTypeEx$(19) = "P-219 Rubblized PCC"
        LayerTypePic$(19) = "P-219 Rubblized PCC"
        LayerTypeList$(19) = "P-219 Rubblized PCC Base"
        DefaultPoisson(19) = 0.35
        DefaultModulus(19) = 75000.0! ' 250000.0! 'IK 2016.05.31
        'ModulusMin(19) = 1000.0! ' 100000.0!
        'ModulusMax(19) = 8000000.0! ' 400000.0!
        DefaultInterface(19) = 0             ' Fully bonded.
        ThickMin(19) = 3.0! ' 4.0!
        DefaultSCIB(19) = 0.0! : SCIBMin(19) = 0.0! : SCIBMax(19) = 0.0!
        DefaultLifeExistPCC(19) = 0.0! : LifeExistPCCMin(19) = 0.0! : LifeExistPCCMax(19) = 0.0!

        LayerType$(20) = NStSBase$
        LayerTypeEx$(20) = "P-401 Stabilized Base (flexible)"
        LayerTypePic$(20) = "P-401/ P-403 St (flex)"
        LayerTypeList$(20) = "P-401/ P-403 HMA"
        DefaultPoisson(20) = 0.35
        DefaultModulus(20) = 400000.0!
        'ModulusMin(20) = 400000.0! : ModulusMax(20) = 400000.0!
        DefaultInterface(20) = 0              ' Fully bonded.
        ThickMin(20) = 4.0!
        DefaultSCIB(20) = 0.0! : SCIBMin(20) = 0.0! : SCIBMax(20) = 0.0!
        DefaultLifeExistPCC(20) = 0.0! : LifeExistPCCMin(20) = 0.0! : LifeExistPCCMax(20) = 0.0!

        LayerType$(21) = NAgBase$ 'PPPP
        LayerTypeEx$(21) = "P-211 Lime Rock Base"
        LayerTypePic$(21) = "P-211 Lime Rock"
        LayerTypeList$(21) = "P-211 Lime Rock"
        DefaultPoisson(21) = 0.35
        DefaultModulus(21) = 75000.0! * 0.8
        'ModulusMin(21) = 1000.0! : ModulusMax(6) = 8000000.0!
        DefaultInterface(21) = 0             ' Fully bonded.
        ThickMin(21) = 4.0!
        DefaultSCIB(21) = 0.0! : SCIBMin(21) = 0.0! : SCIBMax(21) = 0.0!
        DefaultLifeExistPCC(21) = 0.0! : LifeExistPCCMin(21) = 0.0! : LifeExistPCCMax(21) = 0.0!

    End Sub



    Sub StandardMsgs()

        'S = "The pavement structure includes an undefined layer."
        S = "The pavement structure includes an user defined layer."
        SS = "This constitutes a deviation from standards and" & NL
        SS = SS & "requires FAA approval."
        NonStandardStr = S & NL & SS & NL

        '  S$ = "The section does not include a B-777 aircraft." ' GFH 04/07/03.
        '  NoB777$ = S$ & NL & SS$

        S = "The aircraft list contains only one aircraft." & NL
        S = S & "Please see the introduction to the Help File" & NL ' GFH 04/07/03.
        S = S & "for a discussion on using FAArfield to make" & NL
        S = S & "single aircraft comparisons."
        BadACList = S '& NL & SS$ ' GFH 04/07/03.

        S = "The section does not have a design life of " + DefaultLife.ToString() + " years."
        NotDefaultLife = S & NL & SS

    End Sub

End Module