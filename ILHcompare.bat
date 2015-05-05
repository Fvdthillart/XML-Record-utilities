@echo off
cls
title Split Hypotheken XML for compare
set PATH=C:\Windows\Microsoft.NET\Framework64\v4.0.30319;%PATH%

rem Directory waar de baseline staat
rem Directory waar de runresultaten staan
REM set RUNDIR=O:\ILH\DI\OUT\MONTH
set BASELINEDIR=C:\Temp\SVN\ILH_DataIntegratie\Baseline\MONTH
set RUNDIR=C:\Temp\jenkins\MONTH

rem Onderstaande variabele bevatten het timestamp gedeelte van de bestandsnamen.
rem voor elke run
rem BASELINE timestamp
set TS20121231_TEST=1_B_20121231_M_20150204115131
set TS20131130_TEST=1_B_20131130_M_20150204134658

rem JENKINS RUN timestamp
set TS20121231_RUN=1_B_20121231_M_20150209211856
set TS20131130_RUN=1_B_20131130_M_20150209214040

c:
cd C:\temp\SVN\IL_Algemeen\XMLUtils
Rem controleren testruns
extractXMLRecord %BASELINEDIR%\HYPKRO_1_7.%TS20121231_TEST%.xml hyp:Kredietovereenkomst hyp:Homes1Lennr hyp:House1HypothecaireLeningLeningNr hyp:DaybreakAccNbr hyp:VersieNr hyp:Quion1LeningContractNummer hyp:Quion1LeningNummer
extractXMLRecord %RUNDIR%\HYPKRO_1_7.%TS20121231_RUN%.xml  hyp:Kredietovereenkomst hyp:Homes1Lennr hyp:House1HypothecaireLeningLeningNr hyp:DaybreakAccNbr hyp:VersieNr hyp:Quion1LeningContractNummer hyp:Quion1LeningNummer
extractXMLRecord %BASELINEDIR%\HYPKRO_1_7.%TS20131130_TEST%.xml  hyp:Kredietovereenkomst hyp:Homes1Lennr hyp:House1HypothecaireLeningLeningNr hyp:DaybreakAccNbr hyp:VersieNr hyp:Quion1LeningContractNummer hyp:Quion1LeningNummer
extractXMLRecord %RUNDIR%\HYPKRO_1_7.%TS20131130_RUN%.xml  hyp:Kredietovereenkomst hyp:Homes1Lennr hyp:House1HypothecaireLeningLeningNr hyp:DaybreakAccNbr hyp:VersieNr hyp:Quion1LeningContractNummer hyp:Quion1LeningNummer

extractXMLRecord %BASELINEDIR%\HYPPOL_1_7.%TS20121231_TEST%.xml hyp:Polis hyp:Homes1Lvnr hyp:Homes1Lv_my_code hyp:Homes2Aanvrnr hyp:Homes2Lvnr hyp:Homes2Lv_my_code hyp:House1LeningdeelIndicator hyp:House1MortgageapplicationAanvraagId hyp:House1Volgnr hyp:Daybreak1AccNbr hyp:Daybreak1Asbvolgnr hyp:Quion1VerzekeraarNummer hyp:Quion1VerzekeringNummerPolis
extractXMLRecord %RUNDIR%\HYPPOL_1_7.%TS20121231_RUN%.xml hyp:Polis hyp:Homes1Lvnr hyp:Homes1Lv_my_code hyp:Homes2Aanvrnr hyp:Homes2Lvnr hyp:Homes2Lv_my_code hyp:House1LeningdeelIndicator hyp:House1MortgageapplicationAanvraagId hyp:House1Volgnr hyp:Daybreak1AccNbr hyp:Daybreak1Asbvolgnr hyp:Quion1VerzekeraarNummer hyp:Quion1VerzekeringNummerPolis
extractXMLRecord %BASELINEDIR%\HYPPOL_1_7.%TS20131130_TEST%.xml hyp:Polis hyp:Homes1Lvnr hyp:Homes1Lv_my_code hyp:Homes2Aanvrnr hyp:Homes2Lvnr hyp:Homes2Lv_my_code hyp:House1LeningdeelIndicator hyp:House1MortgageapplicationAanvraagId hyp:House1Volgnr hyp:Daybreak1AccNbr hyp:Daybreak1Asbvolgnr hyp:Quion1VerzekeraarNummer hyp:Quion1VerzekeringNummerPolis
extractXMLRecord %RUNDIR%\HYPPOL_1_7.%TS20131130_RUN%.xml hyp:Polis hyp:Homes1Lvnr hyp:Homes1Lv_my_code hyp:Homes2Aanvrnr hyp:Homes2Lvnr hyp:Homes2Lv_my_code hyp:House1LeningdeelIndicator hyp:House1MortgageapplicationAanvraagId hyp:House1Volgnr hyp:Daybreak1AccNbr hyp:Daybreak1Asbvolgnr hyp:Quion1VerzekeraarNummer hyp:Quion1VerzekeringNummerPolis

extractXMLRecord %BASELINEDIR%\HYPSCH_1_7.%TS20121231_TEST%.xml hyp:Schuldenaar hyp:PartijId
extractXMLRecord %RUNDIR%\HYPSCH_1_7.%TS20121231_RUN%.xml hyp:Schuldenaar hyp:PartijId
extractXMLRecord %BASELINEDIR%\HYPSCH_1_7.%TS20131130_TEST%.xml hyp:Schuldenaar hyp:PartijId
extractXMLRecord %RUNDIR%\HYPSCH_1_7.%TS20131130_RUN%.xml hyp:Schuldenaar hyp:PartijId

extractXMLRecord %BASELINEDIR%\HYPTP_1_7.%TS20121231_TEST%.xml hyp:Tussenpersoon hyp:HomesTpnr hyp:SapIcmPorttp hyp:DaybreakProNbr
extractXMLRecord %RUNDIR%\HYPTP_1_7.%TS20131130_RUN%.xml hyp:Tussenpersoon hyp:HomesTpnr hyp:SapIcmPorttp hyp:DaybreakProNbr
extractXMLRecord %BASELINEDIR%\HYPTP_1_7.%TS20131130_TEST%.xml hyp:Tussenpersoon hyp:HomesTpnr hyp:SapIcmPorttp hyp:DaybreakProNbr
extractXMLRecord %RUNDIR%\HYPTP_1_7.%TS20121231_RUN%.xml hyp:Tussenpersoon hyp:HomesTpnr hyp:SapIcmPorttp hyp:DaybreakProNbr

extractXMLRecord %BASELINEDIR%\HYPPTY_1_7.%TS20121231_TEST%.xml hyp:Partij hyp:DaybreakWubCustomerNbr hyp:DaybreakCusSsn hyp:Homes1Relnr hyp:House1BackofficeId hyp:House2MortgageapplicationAanvraagId hyp:House2AanvragerVolgnr hyp:QuionAanvragerNummer
extractXMLRecord %RUNDIR%\HYPPTY_1_7.%TS20121231_RUN%.xml hyp:Partij hyp:DaybreakWubCustomerNbr hyp:DaybreakCusSsn hyp:Homes1Relnr hyp:House1BackofficeId hyp:House2MortgageapplicationAanvraagId hyp:House2AanvragerVolgnr hyp:QuionAanvragerNummer
extractXMLRecord %BASELINEDIR%\HYPPTY_1_7.%TS20131130_TEST%.xml hyp:Partij hyp:DaybreakWubCustomerNbr hyp:DaybreakCusSsn hyp:Homes1Relnr hyp:House1BackofficeId hyp:House2MortgageapplicationAanvraagId hyp:House2AanvragerVolgnr hyp:QuionAanvragerNummer
extractXMLRecord %RUNDIR%\HYPPTY_1_7.%TS20131130_RUN%.xml hyp:Partij hyp:DaybreakWubCustomerNbr hyp:DaybreakCusSsn hyp:Homes1Relnr hyp:House1BackofficeId hyp:House2MortgageapplicationAanvraagId hyp:House2AanvragerVolgnr hyp:QuionAanvragerNummer

extractXMLRecord %BASELINEDIR%\HYPONP_1_7.%TS20121231_TEST%.xml hyp:Onderpand hyp:Homes1Ondpandnr hyp:House1ObjectBackofficeId hyp:House2MortgageapplicationAanvraagId hyp:House2ObjectVolgNr hyp:House3ObkleningLeningNr hyp:Daybreak1AccNbr hyp:Daybreak1Asbvolgnr hyp:Quion1OnderpandNummerZekerheid
extractXMLRecord %RUNDIR%\HYPONP_1_7.%TS20121231_RUN%.xml hyp:Onderpand hyp:Homes1Ondpandnr hyp:House1ObjectBackofficeId hyp:House2MortgageapplicationAanvraagId hyp:House2ObjectVolgNr hyp:House3ObkleningLeningNr hyp:Daybreak1AccNbr hyp:Daybreak1Asbvolgnr hyp:Quion1OnderpandNummerZekerheid
extractXMLRecord %BASELINEDIR%\HYPONP_1_7.%TS20131130_TEST%.xml hyp:Onderpand hyp:Homes1Ondpandnr hyp:House1ObjectBackofficeId hyp:House2MortgageapplicationAanvraagId hyp:House2ObjectVolgNr hyp:House3ObkleningLeningNr hyp:Daybreak1AccNbr hyp:Daybreak1Asbvolgnr hyp:Quion1OnderpandNummerZekerheid
extractXMLRecord %RUNDIR%\HYPONP_1_7.%TS20131130_RUN%.xml hyp:Onderpand hyp:Homes1Ondpandnr hyp:House1ObjectBackofficeId hyp:House2MortgageapplicationAanvraagId hyp:House2ObjectVolgNr hyp:House3ObkleningLeningNr hyp:Daybreak1AccNbr hyp:Daybreak1Asbvolgnr hyp:Quion1OnderpandNummerZekerheid
pause