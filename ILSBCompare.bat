@echo off
cls
title Split XML for compare

set PATH=C:\Windows\Microsoft.NET\Framework64\v4.0.30319;%PATH%
set TS_TEST=0_B_20140630_M_20150224110527
set TS_RUN=0_B_20140630_M_20150225114635
set TESTDIR=C:\Temp\SVN\ILSB_DataIntegratie\Baseline\MONTH
set RUNDIR=P:\ILSB\DI\OUT\MONTH

Rem controleren testruns
extractXMLREcord "%TESTDIR%\SBACO_1_1.%TS_TEST%.XML" sb:AcoPerBron sb:BusinessUnitCode sb:KlantgroepCode sb:ProductCode sb:GeldrekeningId
extractXMLRecord %RUNDIR%\SBACO_1_1.%TS_RUN%.xml sb:AcoPerBron sb:BusinessUnitCode sb:KlantgroepCode sb:ProductCode sb:GeldrekeningId

extractXMLREcord "%TESTDIR%\SBEFF_1_1.%TS_TEST%.XML" sb:Effect sb:EP_BGS_INSTRUMENT_ID sb:EP_REG_INSTRUMENT_ID sb:EP_NN_INSTRUMENT_ID
extractXMLRecord %RUNDIR%\SBEFF_1_1.%TS_RUN%.xml  sb:Effect sb:EP_BGS_INSTRUMENT_ID sb:EP_REG_INSTRUMENT_ID sb:EP_NN_INSTRUMENT_ID

extractXMLREcord "%TESTDIR%\SBINV_1_1.%TS_TEST%.XML" sb:Investeerder sb:PartijId
extractXMLRecord %RUNDIR%\SBINV_1_1.%TS_RUN%.xml sb:Investeerder sb:PartijId

extractXMLREcord "%TESTDIR%\SBIVO_1_1.%TS_TEST%.XML" sb:Investeringsovereenkomst sb:EP_BGS_PRODUCTDETAIL_PRODUCT_ID sb:EP_REG_PRODUCTDETAIL_PRODUCT_ID sb:EP_NN_PRODUCTDETAIL_PRODUCT_ID
extractXMLRecord %RUNDIR%\SBIVO_1_1.%TS_RUN%.xml  sb:Investeringsovereenkomst sb:EP_BGS_PRODUCTDETAIL_PRODUCT_ID sb:EP_REG_PRODUCTDETAIL_PRODUCT_ID sb:EP_NN_PRODUCTDETAIL_PRODUCT_ID

extractXMLREcord "%TESTDIR%\SBKRS_1_1.%TS_TEST%.XML" sb:Effectkoers sb:EffectId
extractXMLRecord %RUNDIR%\SBKRS_1_1.%TS_RUN%.xml sb:Effectkoers sb:EffectId
extractXMLREcord "%TESTDIR%\SBKRS_1_1.%TS_TEST%.XML" sb:Muntkoers sb:MuntsoortCode
extractXMLRecord %RUNDIR%\SBKRS_1_1.%TS_RUN%.xml sb:Muntkoers sb:MuntsoortCode

extractXMLREcord "%TESTDIR%\SBPTY_1_1.%TS_TEST%.XML" il:Partij il:HomesRelnr il:HouseBackofficeId il:HouseAanvraagIdVolgnr il:DaybreakWubCustomerNbr il:DaybreakCusSsn il:EP_BGS_BRON_ID il:EP_BGS_PARTY_ID il:EP_REG_BRON_ID il:EP_REG_PARTY_ID il:EP_NN_BRON_ID il:EP_NN_PARTY_ID
extractXMLRecord %RUNDIR%\SBPTY_1_1.%TS_RUN%.xml il:Partij il:HomesRelnr il:HouseBackofficeId il:HouseAanvraagIdVolgnr il:DaybreakWubCustomerNbr il:DaybreakCusSsn il:EP_BGS_BRON_ID il:EP_BGS_PARTY_ID il:EP_REG_BRON_ID il:EP_REG_PARTY_ID il:EP_NN_BRON_ID il:EP_NN_PARTY_ID

extractXMLREcord "%TESTDIR%\SBTP_1_1.%TS_TEST%.XML" il:Tussenpersoon il:HomesTpnr il:SapIcmPorttp il:DaybreakProNbr il:EP_BGS_BRON_ID il:EP_BGS_PARTY_ID il:EP_REG_BRON_ID il:EP_REG_PARTY_ID il:EP_NN_BRON_ID il:EP_NN_PARTY_ID il:EP_EXTERNALREFERENCE
extractXMLRecord %RUNDIR%\SBTP_1_1.%TS_RUN%.xml il:Tussenpersoon il:HomesTpnr il:SapIcmPorttp il:DaybreakProNbr il:EP_BGS_BRON_ID il:EP_BGS_PARTY_ID il:EP_REG_BRON_ID il:EP_REG_PARTY_ID il:EP_NN_BRON_ID il:EP_NN_PARTY_ID il:EP_EXTERNALREFERENCE

extractXMLREcord "%TESTDIR%\SBTR_1_1.%TS_TEST%.XML" sb:FinancieleTransactie sb:EP_BGS_TRANSACTION_ID sb:EP_REG_TRANSACTION_ID sb:EP_NN_TRANSACTION_ID
extractXMLRecord %RUNDIR%\SBTR_1_1.%TS_RUN%.xml sb:FinancieleTransactie sb:EP_BGS_TRANSACTION_ID sb:EP_REG_TRANSACTION_ID sb:EP_NN_TRANSACTION_ID

pause