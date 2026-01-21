using System.ComponentModel;

namespace URegister.Infrastructure.Constants
{
    public static class NomenclatureTypes
    {
        public const string EkArea1 = "EK001";
        public const string EkArea2 = "EK002";
        public const string EkRegion = "EK003";
        public const string EkMunicipality = "EK004";
        public const string EkTownHall = "EK005";
        public const string Ekatte = "EK006";
        public const string EkRaion = "EK007";
        public const string EkStreet = "EK008";
        public const string EkCountry = "EK009";
        public const string EkKvartal = "EK010";
        public const string EkatteMunicipalityRegion = "EKATTE";
        public const string PidType = "CL0001";
        public const string CidType = "CL0012";
        public const string Status = "CL0010";
        public const string RegixRequestType = "IN0001";
        public const string Currency = "CL0037";
    }
    public static class InternalNomenclatureTypes
    {
        public const string RegisterType = "I0001";
        public const string RegisterEntryType = "I0002";
        public const string RegisterIdentitySecurityLevel = "I0003";
        public const string PersonType = "I0004";
        public const string CodeableConceptStatus = "I0005";
        public const string RegisterStatus = "I0007";
        public const string BlankSourceType = "I0008";
        public const string ChannelType = "I0009";
        public const string DeadlineType = "I0010";
        public const string CalendarDayKind = "I0011";
        public const string DeadlineDayType = "I0012";
        public const string CoordinationStatusType = "I0013";
        public const string OpenDataPeriod = "I0014";
    }

    public static class PersonTypeValue
    {
        public const string Manager = "00001";
        public const string AuthorizedPerson = "00002";
    }
    public static class AdditionalColumnNames
    {
        public const string Nuts3 = "Nuts3";
        public const string Document = "Document";
        public const string Ekatte = "Ekatte";
        public const string Category = "Category";
        public const string Kind = "Kind";
        public const string Kmetstvo = "Kmetstvo";
        public const string TVM = "TVM";
        public const string Altitude = "Altitude";
        public const string Area1 = "Area1";
        public const string Area2 = "Area2";
        public const string CityCode = "CityCode";
    }
    public static class UicTypes
    {
        public const int EGN = 1;
        public const int LNCH = 2;
        public const int EIK = 3;
    }

    public enum PersonRole
    {
        [Description("Партида")]
        Partida = 1,
        [Description("Заявител")]
        Applicant = 2
    }

    public static class RegisterConstants
    {
        public const string CodePrefix = "R";
    }

    /// <summary>
    /// Типове идентификатори за лице
    /// </summary>
    public enum PidTypes
    {
        [Description("ЕГН")]
        EGN = 1,
        [Description("ЛНЧ")]
        LNCH = 2,
        [Description("Паспорт \u2116")]
        PassportNumber = 3,
        [Description("Друго")]
        Other = 4,
        [Description("ЕИК")]
        EIK = 5
    }

    /// <summary>
    /// Роля на лице в заявление
    /// </summary>
    public enum ProcessRole
    {
        Submitter = 1,
        MasterRecordOwner = 2,
    }

    public enum ProcessStatus
    {
        [Description("Изпратени данни")]
        Send = 1,
        [Description("В процес на обработка")]
        InWork = 2,
        [Description("Регистрирани данни")]
        Registered = 3,
        [Description("Отхвърлен")]
        Refused = 4,
        [Description("Издадено удостоверение")]
        Certificate = 5,
        [Description("Указания")]
        Instruction = 6,
        [Description("За съгласуване")]
        ForCoordination = 7,
        [Description("Съгласуван")]
        Coordination = 8,
        [Description("В процес на подписване")]
        Signing = 9,
        [Description("Указания в процес на подписване")]
        InstructionSigning = 10,
        [Description("Отхвърлен в процес на подписване")]
        RefusedSigning = 11,

    }

    public enum CodeableConceptStatus
    {
        New = 1,
        Confirmed = 2,
        Refused = 3,
    }

    public enum ApprovalStatus
    {
        [Description("Заявен")]
        Requested = 1,
        [Description("Одобрен")]
        Approved = 2,
        [Description("Отхвърлено")]
        Rejected = 3,
    }
    public enum ServiceTypes
    {
        [Description("Вписване")]
        Register = 1,
        [Description("Издаване на документ")]
        Document = 2,
        [Description("Искане за изправяне на грешки")]
        AskForCorrectionError = 3,
        [Description("Промяна на данни")]
        Change = 4,
        [Description("Заличаване")]
        Deletion = 5 
    }
    public enum ServiceSteps
    {
        [Description("Съгласуване")]
        Coordination = 2,
    }

    /// <summary>
    /// Типове идентификатори за лице
    /// </summary>
    public enum CidTypes
    {
        [Description("ЕИК")]
        EIK = 1,
        [Description("БУЛСТАТ")]
        BULSTAT = 2,
        [Description("Чуждестранно ЮЛ")]
        ForeignCompany = 3
    }

    /// <summary>
    /// Правна форма
    /// </summary>
    public enum LegalFormsEIK
    {
        [Description("ЕТ")]
        SoleTrader = 1,

        [Description("СД")]
        GeneralPartnership = 2,

        [Description("КД")]
        LimitedPartnership = 3,

        [Description("ООД")]
        LimitedLiabilityCompanyLLC = 4,

        [Description("АД")]
        JointStockCompanyJSC = 5,

        [Description("КДА")]
        LimitedPartnershipWithShares = 6,

        [Description("Кооперация")]
        Cooperative = 7,

        [Description("КЧТ")]
        BranchOfForeignMerchant = 8,

        [Description("ТПП")]
        ChamberOfCommerceAndIndustry = 9,

        [Description("ЕООД")]
        SoleMemberLLC = 10,

        [Description("ЕАД")]
        SingleMemberJointStockCompany = 11,

        [Description("АДСИЦ")]
        JointStockCompanyWithSpecialInvestmentPurpose = 12,

        [Description("ДП")]
        StateEnterprise = 13,

        [Description("ОП")]
        CommunityEnterprise = 14,

        [Description("ЕАДСИЦ")]
        SingleMemberJointStockCompanyWithSpecialInvestmentPurpose = 15,

        [Description("ЕОИИ")]
        EuropeanEconomicInterestGrouping = 16,

        [Description("Поделение на ЕОИИ")]
        DivisionOfEuropeanEconomicInterestGrouping = 17,

        [Description("ЕД")]
        SocietasEuropaeaSE = 18,

        [Description("ЕКД")]
        EuropeanCooperativeSociety = 19,

        [Description("ЕКДОО")]
        EuropeanCooperativeLimitedLiabilityCompany = 20,

        [Description("ЧДПР")]
        CompaniesInJurisdictionsWithPreferentialTaxRegime = 21,

        [Description("ЧЮЛ")]
        ForeignLegalEntity = 22,

        [Description("ЧФЛ")]
        ForeignNaturalPerson = 23,

        [Description("Сдружение")]
        Association = 24,

        [Description("Фондация")]
        Foundation = 25,

        [Description("Клон на ЧЮЛНЦ")]
        BranchOfForeignNonProfitLegalEntity = 26,

        [Description("НЧ")]
        FolkCommunityCenter = 27,

        [Description("Неизвестно")]
        Unknown = 28
    }

    public enum LegalFormsBULSTAT
    {
        [Description("Еднолично акционерно дружество със специална инвестиционна цел")]
        SoleProprietorshipWithSpecialInvestmentPurpose = 15,

        [Description("Европейско обединение по икономически интереси")]
        EuropeanEconomicInterestGrouping = 16,

        [Description("Поделение на ЕОИИ")]
        DivisionOfEEIG = 17,

        [Description("Европейско дружество")]
        EuropeanCompany = 18,

        [Description("Европейско кооперативно дружество")]
        EuropeanCooperativeSociety = 19,

        [Description("Европейско кооперативно дружество с ограничена отговорност")]
        EuropeanCooperativeSocietyWithLimitedLiability = 20,

        [Description("Дружество регистрирано в юрисдикция с преференциален данъчен режим")]
        CompanyRegisteredInPreferentialTaxRegime = 21,

        [Description("Субект на СРБ - юридическо лице")]
        SubjectOfSRB_LegalEntity = 229,

        [Description("Субект на СРБ - неюридическо лице")]
        SubjectOfSRB_NonLegalEntity = 230,

        [Description("Събирателно дружество")]
        Partnership = 455,

        [Description("Дружество с ограничена отговорност")]
        LimitedLiabilityCompany = 456,

        [Description("Еднолично дружество с ограничена отговорност")]
        SoleLimitedLiabilityCompany = 457,

        [Description("Акционерно дружество")]
        JointStockCompany = 458,

        [Description("Еднолично акционерно дружество")]
        SoleProprietorship = 459,

        [Description("Командитно дружество")]
        LimitedPartnership = 460,

        [Description("Командитно дружество с акции")]
        LimitedPartnershipWithShares = 461,

        [Description("Едноличен търговец")]
        SoleTrader = 462,

        [Description("Държавна фирма")]
        StateCompany = 463,

        [Description("Общинска фирма")]
        MunicipalCompany = 464,

        [Description("Дъщерна фирма")]
        Subsidiary = 465,

        [Description("Кооперация")]
        Cooperative = 466,

        [Description("Кооперативен съюз")]
        CooperativeUnion = 467,

        [Description("Кооперативна федерация")]
        CooperativeFederation = 468,

        [Description("Кооперативно предприятие")]
        CooperativeEnterprise = 469,

        [Description("Междукооперативно предприятие")]
        InterCooperativeEnterprise = 470,

        [Description("Жилищностроителна кооперация")]
        HousingCooperative = 471,

        [Description("Дружество по ЗЗД")]
        PartnershipUnderOCA = 472,

        [Description("Предприятие (по ПМС 50/68)")]
        EnterpriseUnderDecree5068 = 473,

        [Description("Публично предприятие, създадено с спец закон")]
        PublicEnterpriseBySpecialLaw = 474,

        [Description("Клон на чуждестранен търговец")]
        BranchOfForeignTrader = 475,

        [Description("Търговско представителство")]
        CommercialRepresentation = 476,

        [Description("Представителство на международна организация")]
        RepresentationOfInternationalOrganization = 477,

        [Description("Дипломатическо представителство")]
        DiplomaticMission = 478,

        [Description("Консулско представителство")]
        ConsularRepresentation = 479,

        [Description("Представителство на чуждестранно радио/телевизия")]
        RepresentationOfForeignRadioTV = 480,

        [Description("Представителство на чуждестранна агенция по печат")]
        RepresentationOfForeignNewsAgency = 481,

        [Description("Представителство на чуждестранна авиокомпания")]
        RepresentationOfForeignAirline = 482,

        [Description("Представителство на друго чуждестранно лице")]
        RepresentationOfAnotherForeignEntity = 483,

        [Description("Културен център на друга държава")]
        CulturalCenterOfAnotherCountry = 484,

        [Description("Фондация")]
        Foundation = 485,

        [Description("Сдружение")]
        Association = 486,

        [Description("Синдикална организация")]
        TradeUnionOrganization = 487,

        [Description("Народно читалище")]
        CommunityCenter = 488,

        [Description("Политическа организация")]
        PoliticalOrganization = 489,

        [Description("Религиозна организация")]
        ReligiousOrganization = 490,

        [Description("Адвокатска колегия")]
        BarAssociation = 491,

        [Description("Нотариална камара")]
        NotaryChamber = 492,

        [Description("Неправителствена организация, създадена с нарочно основание")]
        NGOWithSpecialGrounds = 493,

        [Description("Администрация на президента на РБългария")]
        AdministrationOfPresidentOfBulgaria = 494,

        [Description("Народно събрание")]
        NationalAssembly = 495,

        [Description("Институция, създадена със специален закон")]
        InstitutionBySpecialLaw = 496,

        [Description("Министерски съвет")]
        CouncilOfMinisters = 497,

        [Description("Министерство")]
        Ministry = 498,

        [Description("Комитет")]
        Committee = 499,

        [Description("Агенция")]
        Agency = 500,

        [Description("Областен управител")]
        RegionalGovernor = 501,

        [Description("Съд")]
        Court = 502,

        [Description("Прокуратура")]
        ProsecutorsOffice = 503,

        [Description("Следствена служба")]
        InvestigativeService = 504,

        [Description("Община")]
        Municipality = 505,

        [Description("Кметство")]
        MayorsOffice = 506,

        [Description("Театър")]
        Theater = 507,

        [Description("Опера")]
        Opera = 508,

        [Description("Музей, галерия")]
        MuseumGallery = 509,

        [Description("Филхармония")]
        Philharmonic = 510,

        [Description("Ансамбъл")]
        Ensemble = 511,

        [Description("Цирк")]
        Circus = 512,

        [Description("Кино")]
        Cinema = 513,

        [Description("Библиотека")]
        Library = 514,

        [Description("Научна организация, създадена с спец закон")]
        ScientificOrganizationBySpecialLaw = 515,

        [Description("Научен институт")]
        ScientificInstitute = 516,

        [Description("Висше училище")]
        HigherEducationInstitution = 517,

        [Description("Организация в състава на ВУ")]
        OrganizationWithinHEI = 518,

        [Description("Колеж извън състава на ВУ")]
        CollegeOutsideHEI = 519,

        [Description("Колеж в състава на ВУ")]
        CollegeWithinHEI = 520,

        [Description("Училище")]
        School = 521,

        [Description("Детска градина")]
        Kindergarten = 522,

        [Description("Амбулаторно-поликлинично заведение")]
        OutpatientClinic = 523,

        [Description("Болнично заведение")]
        Hospital = 524,

        [Description("Диспансер")]
        Dispensary = 525,

        [Description("Здравно заведение за опазване здравето на майката и детето")]
        HealthInstitutionForMotherAndChild = 526,

        [Description("Национален център за опазване на общественото здраве")]
        NationalCenterForPublicHealth = 527,

        [Description("Санaтoрно-курортно и лечебно-оздравително заведение")]
        SanatoriumAndHealthResort = 528,

        [Description("Клон")]
        Branch = 529,

        [Description("Поделение")]
        Division = 530,

        [Description("Общинско предприятие")]
        MunicipalEnterprise = 531,

        [Description("Физическо лице - субект на Булстат")]
        PhysicalPersonSubjectOfBulstat = 532,

        [Description("Юридическо лице в сферата на държавната власт")]
        LegalEntityInStatePower = 533,

        [Description("Друг вид субект")]
        OtherTypeOfEntity = 534,

        [Description("Чуждестранно лице, нерегистрирано в България")]
        ForeignPersonNotRegisteredInBulgaria = 535,

        [Description("друг вид нефизическо лице без право на ЕИК")]
        OtherNonPhysicalPersonWithoutUIC = 536,

        [Description("Централна банка")]
        CentralBank = 1003,

        [Description("Държавна спестовна каса")]
        StateSavingsBank = 1185,

        [Description("Държавeн застрахователен институт")]
        StateInsuranceInstitute = 1186,

        [Description("Областна администрация")]
        RegionalAdministration = 1187,

        [Description("Полувисш институт")]
        IntermediateInstitute = 1188,

        [Description("Място за лишаване от свобода")]
        PlaceOfDeprivationOfLiberty = 1189,

        [Description("Взаимноспомагателна каса")]
        MutualAidFund = 1190,

        [Description("Юридическо лице към културна институция")]
        LegalEntityAtCulturalInstitution = 1191,

        [Description("Юридическо лице към научна организация")]
        LegalEntityAtScientificOrganization = 1192,

        [Description("Обслужващо звено в системата на образованието")]
        ServiceUnitInEducationSystem = 1193,

        [Description("Районна колегия")]
        RegionalCollege = 1200,

        [Description("Национална здравно-осигурителна каса")]
        NationalHealthInsuranceFund = 1215,

        [Description("Държавна агенция")]
        StateAgency = 1216,

        [Description("Държавна комисия")]
        StateCommission = 1217,

        [Description("Изпълнителна агенция")]
        ExecutiveAgency = 1218,

        [Description("Лечебно заведение за извънболнична помощ")]
        MedicalInstitutionForOutpatientCare = 1219,

        [Description("Лечебно заведение за болнична помощ")]
        MedicalInstitutionForHospitalCare = 1220,

        [Description("Друго лечебно заведение")]
        OtherMedicalInstitution = 1221,

        [Description("Хигиенно-епидемиологична инспекция")]
        HygieneAndEpidemiologicalInspection = 1222,

        [Description("Културен институт")]
        CulturalInstitute = 1223,

        [Description("Чуждестранно нефизическо лице с място на стопанска дейност в страната")]
        ForeignNonPhysicalPersonWithEconomicActivity = 1234,

        [Description("Чуждестранно нефизическо лице - наемодател")]
        ForeignNonPhysicalPersonLandlord = 1249,

        [Description("Пенсионен фонд")]
        PensionFund = 1300,

        [Description("Клон на чуждестранно юридическо лице с нестопанска цел")]
        BranchOfForeignNonProfitLegalEntity = 1307,

        [Description("Сдружение за напояване")]
        IrrigationAssociation = 1317,

        [Description("Фирма на обществена организация")]
        CompanyOfPublicOrganization = 1322,

        [Description("Държавно предприятие")]
        StateEnterprise = 1324,

        [Description("Чуждестранно юридическо лице, притежаващо имущество в страната")]
        ForeignLegalEntityOwningProperty = 1329,

        [Description("Религиозна институция")]
        ReligiousInstitution = 1342,

        [Description("Местно поделение (ЮЛ)")]
        LocalDivisionLegalEntity = 1348,

        [Description("Занаятчийско предприятие")]
        CraftEnterprise = 1509,

        [Description("Религиозна институция (БПЦ)")]
        ReligiousInstitutionBPC = 1515,

        [Description("Местно поделение на БПЦ (ЮЛ)")]
        LocalDivisionOfBPC_LegalEntity = 1516,

        [Description("Адвокатско дружество")]
        LawFirm = 1533,

        [Description("Адвокатско съдружие")]
        LawPartnership = 1534,

        [Description("Компенсационен фонд")]
        CompensationFund = 1537,

        [Description("Акционерно дружество със специална инвестиционна цел")]
        SpecialPurposeJointStockCompany = 1540,

        [Description("Договорен фонд")]
        ContractualFund = 1559,

        [Description("Чуждестранно юридическо лице с ефективно управление")]
        ForeignLegalEntityWithEffectiveManagement = 1566,

        [Description("Нефизическо лице - осигурител")]
        NonPhysicalPersonInsurer = 1575,

        [Description("Чуждестранно лице регистрирано по ЗДДС")]
        ForeignPersonRegisteredUnderVATAct = 1579,

        [Description("Читалищно сдружение")]
        CommunityCenterAssociation = 1586,

        [Description("Съюз на народните читалища")]
        UnionOfCommunityCenters = 1587,

        [Description("Клон на Чуждестранно Адвокатско дружество")]
        BranchOfForeignLawFirm = 1588,

        [Description("Регионално сдружение")]
        RegionalAssociation = 1590,

        [Description("Сдружение на собствениците")]
        OwnersAssociation = 1592,

        [Description("Чужд. НФЛ - осигурител")]
        ForeignNPLInsurer = 1598,

        [Description("Чужд. лице по чл. 3 ал. 2 от ЗРБ")]
        ForeignPersonUnderArt3Para2 = 1600,

        [Description("Център за подкрепа за личностно развитие")]
        CenterForPersonalDevelopmentSupport = 2002,

        [Description("Специализирано обслужващо звено")]
        SpecializedServiceUnit = 2003,

        [Description("Орг. за управление на туристически район")]
        OrganizationForManagementOfTouristArea = 2010,

        [Description("Център за специална образователна подкрепа")]
        CenterForSpecialEducationalSupport = 2011,

        [Description("Научен център")]
        ScientificCenter = 2012,

        [Description("Чуждестранно юридическо лице - залогодател")]
        ForeignLegalEntityPledger = 2014,

        [Description("Работодателска организация")]
        EmployerOrganization = 2017,

        [Description("Фонд за разсрочени плащания")]
        FundForDeferredPayments = 2019,

        [Description("Фонд за изплащане на пожизнени пенсии")]
        FundForLifetimePensionPayments = 2020,

        [Description("Доверителен собственик по чл. 3, ал. 3 от ЗРБ")]
        TrustOwnerUnderArt3Para3 = 2022
    }

    public enum FileSourceType
    {
        [Description("Прикачен документ в заявление")]
        AttachedDocument = 1,
        [Description("Чернова на удостоверение")]
        CertificateDraft = 2,
        [Description("Подпечатано удостоверение")]
        Certificate = 3,
        [Description("Чернова на отказ")]
        RefuseDraft = 4,
        [Description("Подпечатан отказ")]
        Refuse = 5,
        [Description("Чернова на указание")]
        InstructionDraft = 6,
        [Description("Подпечатанo указание")]
        Instruction = 7,
        [Description("Прикачен документ в изпълнение на указания")]
        InstructionResponse = 8,
        [Description("Е-Форма")]
        EFormApplication = 9,
        [Description("Импортиран файл")]
        Import = 10,
        [Description("Макет за импортиране")]
        ImportMaket = 11,
    }

    public enum RegisterStatusType
    {
        [Description("Въведен")]
        Draft = 1,
        [Description("Одобрен")]
        Register = 2,
        [Description("Деактивиран")]
        Deleted = 3,
    }

    public enum RegixRequestTypes
    {
        [Description("Справка за физическо лице")]
        DataRequestForPerson = 1,
        [Description("Справка за юридическо лице")]
        DataRequestForCompany = 2,
    }

    public enum BlankSourceType
    {
        [Description("Удостоверение")]
        Certicicate = 1,
        [Description("Удостоверение при вписване")]
        CertificateOnRegister = 2,
        [Description("Отказ")]
        Refuse = 3,
        [Description("Указание")]
        Instruction = 4,
    }

    public enum EDeliveryStep
    {
        [Description("Прочетен от списък")]
        List = 1,
        [Description("Приет")]
        Open = 2,
        [Description("Прочетен файл")]
        File = 3,
        [Description("Входиран")]
        Process = 4
    }
    public enum EDeliveryStatus
    {
        [Description("В обработка")]
        InWork = 1,
        [Description("Грешка")]
        Error = 2,
        [Description("Приключен")]
        Ready = 3,
    }
    public enum EDeliveryMessageType
    {
        [Description("Заявление")]
        Application = 1,
        [Description("Отговор на указания")]
        InstructionResponse = 4,
        [Description("Подадено заявление уведомление")]
        OutApplication = 10,
        [Description("Удостоверение")]
        OutCertificate = 11,
        [Description("Отказ")]
        OutRefuse = 12,
        [Description("Указания")]
        OutInstruction = 13,
        [Description("Вписване")]
        RegisterApplication = 14,
        [Description("Друго")]
        Other = 99,
    }
    public static class EDeliveryMessageTypeConsts {
        public static int[] EDeliveryMessageTypeOut = new int[]{
           (int)EDeliveryMessageType.OutApplication,
           (int)EDeliveryMessageType.OutCertificate,
           (int)EDeliveryMessageType.OutRefuse,
           (int)EDeliveryMessageType.OutInstruction,
    };
    }

    public enum EDeliveryFileType
    {
        [Description("Заявление")]
        Application = 1,
        [Description("Декларация")]
        Declaration = 2,
        [Description("Прикачен файл")]
        AttachedFile = 3,
    }

    public enum IntegrationSourceType
    {
        [Description("Отговор на указания")]
        InstructionResponse = 4,
        [Description("Указания")]
        Instruction = 13,
        [Description("Удостоверение")]
        Certificate = 11,
        [Description("Отказ")]
        Refuse = 12,
    }
    public static class GlobalConsts {
        public const bool ShowNomenclatureHolder = false;
        public const bool ShowNomenclatureAdditional = false;
        public const bool ShowBlankCode = false;
    }
    public enum RegisterFileSourceType
    {
        [Description("Регистър")]
        Register = 1,
        [Description("Администрация")]
        Administration = 2,
        [Description("Статус")]
        RegisterStatus = 3,
    }
    public static class ChannelType
    {
        public const string EDelivery = "0006-000080";
        public const string OnDesk = "0006-000077";
        public const string OnEmail = "0006-000076";
    }
    public static class CalendarDayKind
    {
        public const string WorkDay = "1";
        public const string NotWorkingDay = "2";
    }
    public static class DeadlineDayType
    {
        public const string WorkDay = "1006-130001";
        public const string CalendarDay = "1006-130002";
    }

    public static class RegisterTypeEntry {

        /// <summary>
        /// От заявител
        /// </summary>
        public const string Applicant = "00001";

        /// <summary>
        /// Служебно
        /// </summary>
        public const string Officially = "00002";
    }

    public static class RegisterType
    {
        /// <summary>
        /// Публичен
        /// </summary>
        public const string Public = "00001";

        /// <summary>
        /// Служебен
        /// </summary>
        public const string Official = "00002";
    }

    public enum EMailStatus
    {
        [Description("Нов/за изпращане")]
        New = 1,
        [Description("Грешка")]
        Error = 2,
        [Description("Изпратен")]
        Send = 3,
    }

    public enum EMailSourceType
    {
        [Description("Нов/за изпращане")]
        ReceivedEForm = 1,
        [Description("Предупрежедение за настъпил срок за вписване на заявление")]
        SrokForApplication = 71,

    }

    public enum Currency
    {
        [Description("Български лев")]
        BGN = 1,
        [Description("Евро")]
        EUR = 2,
    }

    public enum OpenDataPeriod
    {
        [Description("Не се изпраща")]
        Not = -1,

        [Description("Като администрацията")]
        Administration = 0,

        [Description("Ежедневно")]
        Day = 1,
        [Description("Седмично")]
        Week = 2,
        [Description("Седмично")]
        Month = 3,
    }
}
