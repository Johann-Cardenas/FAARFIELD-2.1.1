Module modAdvisoryCircularRq

    Sub AC_6E_note_320_NewFlex()
        AC_Note320 = False
        If Aircraft_100t_lbs Then

            If Not (LCode(2) = 14 Or LCode(2) = 16 Or LCode(2) = 17) Then
                '14 for P-401 St (flex)
                '15 for P-301 Soil Cement Base (???)
                '16 for P-304 Cement Treated Base
                '17 for P-306 Econoconcrete
                '19 for Rubblized Layer
                'Title = "AC 150/5320-6E Note"
                Note320 = "AC 150/5320-6E Note" + NL
                Note320 = Note320 + "§316. STABILIZED BASE AND SUBBASE." + NL + NL
                Note320 = Note320 + "Stabilized base and subbase courses are necessary" + NL
                Note320 = Note320 + "for new pavements designed to accomodate jet aicraft " + NL
                Note320 = Note320 + "weighting 100,000 pounds (45,350 kg) or more. These" + NL
                Note320 = Note320 + "stabilized courses may be substituted for granular" + NL
                Note320 = Note320 + "courses using the equivalency factors discussed " + NL
                Note320 = Note320 + "in paragraph 322." + NL
                AC_Note320 = True
            End If

        End If
    End Sub

    Sub AC_6E_note_328_NewRigid()
        AC_Note328 = False
        If Aircraft_100t_lbs Then
            If Not (LCode(2) = 14 Or LCode(2) = 16 Or LCode(2) = 17 Or LCode(2) = 19) Then
                '14 for P-401 St (flex)
                '15 for P-301 Soil Cement Base (???)
                '16 for P-304 Cement Treated Base
                '17 for P-306 Econoconcrete
                '19 for Rubblized Layer

                'Title = "AC 150/5320-6E Note"
                Note328 = "AC 150/5320-6E Note" + NL
                Note328 = Note328 + "§323. STABILIZED SUBBASE." + NL + NL
                Note328 = Note328 + "Stabilized subbase is required for all new rigid" + NL
                Note328 = Note328 + "pavements designed to accomodate aicraft " + NL
                Note328 = Note328 + "weighting 100,000 pounds (45,400 kg) or more." + NL
                Note328 = Note328 + "Stabilized subbases are as follows:" + NL + NL
                Note328 = Note328 + "Item P-304 - Cement Treated Base Course" + NL
                Note328 = Note328 + "Item P-306 - Econoconcrete Subbase Course" + NL
                Note328 = Note328 + "Item P-401 - Plant Mix Bituminous Pavements" + NL
                AC_Note328 = True
            End If
        End If
    End Sub

End Module
