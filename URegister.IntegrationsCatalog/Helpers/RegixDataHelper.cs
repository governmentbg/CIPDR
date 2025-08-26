namespace URegister.IntegrationsCatalog.Helpers
{
    public class RegixDataHelper
    {
        public static Dictionary<string, string> CountryCodeMapEIK =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "AXX", "A" }, // Custom code for single-letter "A"
                { "AAX", "AA" }, // Custom code for non-standard "AA"
                { "AND", "AD" }, // Andorra
                { "ARE", "AE" }, // United Arab Emirates
                { "AFG", "AF" }, // Afghanistan
                { "ATG", "AG" }, // Antigua and Barbuda
                { "AIA", "AI" }, // Anguilla
                { "ALB", "AL" }, // Albania
                { "ARM", "AM" }, // Armenia
                { "AGO", "AO" }, // Angola
                { "ATA", "AQ" }, // Antarctica
                { "ARG", "AR" }, // Argentina
                { "ASM", "AS" }, // American Samoa
                { "AUT", "AT" }, // Austria
                { "AUS", "AU" }, // Australia
                { "ABW", "AW" }, // Aruba
                { "AZE", "AZ" }, // Azerbaijan
                { "BXX", "B" }, // Custom code for single-letter "B"
                { "BIH", "BA" }, // Bosnia and Herzegovina
                { "BRB", "BB" }, // Barbados
                { "BGD", "BD" }, // Bangladesh
                { "BEL", "BE" }, // Belgium
                { "BFA", "BF" }, // Burkina Faso
                { "BGR", "BG" }, // Bulgaria
                { "BHR", "BH" }, // Bahrain
                { "BDI", "BI" }, // Burundi
                { "BEN", "BJ" }, // Benin
                { "BMU", "BM" }, // Bermuda
                { "BOL", "BO" }, // Bolivia
                { "BRA", "BR" }, // Brazil
                { "BHS", "BS" }, // Bahamas
                { "BTN", "BT" }, // Bhutan
                { "BUM", "BU" }, // Burma (historical, now Myanmar, "MM")
                { "BWA", "BW" }, // Botswana
                { "BLR", "BY" }, // Belarus
                { "BLZ", "BZ" }, // Belize
                { "CXX", "C" }, // Custom code for single-letter "C"
                { "CAN", "CA" }, // Canada
                { "CCK", "CC" }, // Cocos (Keeling) Islands
                { "CEX", "CE" }, // Custom code for non-standard "CE"
                { "CAF", "CF" }, // Central African Republic
                { "COG", "CG" }, // Congo
                { "CHE", "CH" }, // Switzerland
                { "CIV", "CI" }, // Côte d'Ivoire
                { "COK", "CK" }, // Cook Islands
                { "CHL", "CL" }, // Chile
                { "CMR", "CM" }, // Cameroon
                { "COL", "CO" }, // Colombia
                { "CRI", "CR" }, // Costa Rica
                { "SCG", "CS" }, // Serbia and Montenegro (historical)
                { "CUB", "CU" }, // Cuba
                { "CPV", "CV" }, // Cabo Verde
                { "CXR", "CX" }, // Christmas Island
                { "CYP", "CY" }, // Cyprus
                { "CZE", "CZ" }, // Czechia
                { "DEU", "DE" }, // Germany
                { "DJI", "DJ" }, // Djibouti
                { "DNK", "DK" }, // Denmark
                { "DMA", "DM" }, // Dominica
                { "DOM", "DO" }, // Dominican Republic
                { "DZA", "DZ" }, // Algeria
                { "ECU", "EC" }, // Ecuador
                { "EST", "EE" }, // Estonia
                { "EGY", "EG" }, // Egypt
                { "ESH", "EH" }, // Western Sahara
                { "ERI", "ER" }, // Eritrea
                { "ESP", "ES" }, // Spain
                { "ETH", "ET" }, // Ethiopia
                { "FIN", "FI" }, // Finland
                { "FJI", "FJ" }, // Fiji
                { "FLK", "FK" }, // Falkland Islands
                { "FSM", "FM" }, // Micronesia
                { "FRO", "FO" }, // Faroe Islands
                { "FRA", "FR" }, // France
                { "FXX", "FX" }, // Metropolitan France (historical)
                { "GXX", "G" }, // Custom code for single-letter "G"
                { "GAB", "GA" }, // Gabon
                { "GBR", "GB" }, // United Kingdom
                { "GRD", "GD" }, // Grenada
                { "GEO", "GE" }, // Georgia
                { "GUF", "GF" }, // French Guiana
                { "GHA", "GH" }, // Ghana
                { "GIB", "GI" }, // Gibraltar
                { "GRL", "GL" }, // Greenland
                { "GMB", "GM" }, // Gambia
                { "GNB", "GW" }, // Guinea-Bissau
                { "GLP", "GP" }, // Guadeloupe
                { "GNQ", "GQ" }, // Equatorial Guinea
                { "GRC", "GR" }, // Greece
                { "SGS", "GS" }, // South Georgia and South Sandwich Islands
                { "GTM", "GT" }, // Guatemala
                { "GUM", "GU" }, // Guam
                { "GUY", "GY" }, // Guyana
                { "HXX", "H" }, // Custom code for single-letter "H"
                { "HKG", "HK" }, // Hong Kong
                { "HMD", "HM" }, // Heard Island and McDonald Islands
                { "HRV", "HR" }, // Croatia
                { "HTI", "HT" }, // Haiti
                { "HUN", "HU" }, // Hungary
                { "IXX", "I" }, // Custom code for single-letter "I"
                { "IDN", "ID" }, // Indonesia
                { "IRL", "IE" }, // Ireland
                { "ISR", "IL" }, // Israel
                { "IOT", "IO" }, // British Indian Ocean Territory
                { "IRQ", "IQ" }, // Iraq
                { "IRN", "IR" }, // Iran
                { "ISL", "IS" }, // Iceland
                { "ITA", "IT" }, // Italy
                { "JAM", "JM" }, // Jamaica
                { "JOR", "JO" }, // Jordan
                { "JPN", "JP" }, // Japan
                { "KXX", "K" }, // Custom code for single-letter "K"
                { "KEN", "KE" }, // Kenya
                { "KGZ", "KG" }, // Kyrgyzstan
                { "KHM", "KH" }, // Cambodia
                { "KIR", "KI" }, // Kiribati
                { "COM", "KM" }, // Comoros
                { "PRK", "KP" }, // Korea, North
                { "KOR", "KR" }, // Korea, South
                { "KWT", "KW" }, // Kuwait
                { "CYM", "KY" }, // Cayman Islands
                { "KAZ", "KZ" }, // Kazakhstan
                { "LAO", "LA" }, // Laos
                { "LBN", "LB" }, // Lebanon
                { "LCA", "LC" }, // Saint Lucia
                { "LIE", "LI" }, // Liechtenstein
                { "LKA", "LK" }, // Sri Lanka
                { "LBR", "LR" }, // Liberia
                { "LSO", "LS" }, // Lesotho
                { "LTU", "LT" }, // Lithuania
                { "LUX", "LU" }, // Luxembourg
                { "LVA", "LV" }, // Latvia
                { "LBY", "LY" }, // Libya
                { "MXX", "M" }, // Custom code for single-letter "M"
                { "MAR", "MA" }, // Morocco
                { "MCO", "MC" }, // Monaco
                { "MDA", "MD" }, // Moldova
                { "MNE", "ME" }, // Montenegro
                { "MDG", "MG" }, // Madagascar
                { "MHL", "MH" }, // Marshall Islands
                { "MIX", "MI" }, // Custom code for non-standard "MI"
                { "MKD", "MK" }, // North Macedonia
                { "MLI", "ML" }, // Mali
                { "MMR", "MM" }, // Myanmar
                { "MAC", "MO" }, // Macao
                { "MNP", "MP" }, // Northern Mariana Islands
                { "MTQ", "MQ" }, // Martinique
                { "MRT", "MR" }, // Mauritania
                { "MSR", "MS" }, // Montserrat
                { "MLT", "MT" }, // Malta
                { "MUS", "MU" }, // Mauritius
                { "MDV", "MV" }, // Maldives
                { "MWI", "MW" }, // Malawi
                { "MEX", "MX" }, // Mexico
                { "MYS", "MY" }, // Malaysia
                { "MOZ", "MZ" }, // Mozambique
                { "NAM", "NA" }, // Namibia
                { "NCL", "NC" }, // New Caledonia
                { "NER", "NE" }, // Niger
                { "NFK", "NF" }, // Norfolk Island
                { "NGA", "NG" }, // Nigeria
                { "NIC", "NI" }, // Nicaragua
                { "NLD", "NL" }, // Netherlands
                { "NOR", "NO" }, // Norway
                { "NPL", "NP" }, // Nepal
                { "NRU", "NR" }, // Nauru
                { "NIU", "NU" }, // Niue
                { "NZL", "NZ" }, // New Zealand
                { "OMN", "OM" }, // Oman
                { "PXX", "P" }, // Custom code for single-letter "P"
                { "PAN", "PA" }, // Panama
                { "PER", "PE" }, // Peru
                { "PYF", "PF" }, // French Polynesia
                { "PNG", "PG" }, // Papua New Guinea
                { "PHL", "PH" }, // Philippines
                { "PAK", "PK" }, // Pakistan
                { "POL", "PL" }, // Poland
                { "SPM", "PM" }, // Saint Pierre and Miquelon
                { "PRI", "PR" }, // Puerto Rico
                { "PSE", "PS" }, // Palestine
                { "PRT", "PT" }, // Portugal
                { "PUX", "PU" }, // Custom code for non-standard "PU"
                { "PLW", "PW" }, // Palau
                { "PRY", "PY" }, // Paraguay
                { "QAT", "QA" }, // Qatar
                { "REU", "RE" }, // Réunion
                { "ROU", "RO" }, // Romania
                { "SRB", "RS" }, // Serbia
                { "RUS", "RU" }, // Russia
                { "RWA", "RW" }, // Rwanda
                { "SXX", "S" }, // Custom code for single-letter "S"
                { "SAU", "SA" }, // Saudi Arabia
                { "SLB", "SB" }, // Solomon Islands
                { "SYC", "SC" }, // Seychelles
                { "SDN", "SD" }, // Sudan
                { "SWE", "SE" }, // Sweden
                { "SGP", "SG" }, // Singapore
                { "SHN", "SH" }, // Saint Helena
                { "SVN", "SI" }, // Slovenia
                { "SJM", "SJ" }, // Svalbard and Jan Mayen
                { "SVK", "SK" }, // Slovakia
                { "SLE", "SL" }, // Sierra Leone
                { "SMR", "SM" }, // San Marino
                { "SOM", "SO" }, // Somalia
                { "SUR", "SR" }, // Suriname
                { "STP", "ST" }, // São Tomé and Príncipe
                { "SUN", "SU" }, // Soviet Union (historical)
                { "SLV", "SV" }, // El Salvador
                { "SYR", "SY" }, // Syria
                { "SWZ", "SZ" }, // Eswatini
                { "TXX", "T" }, // Custom code for single-letter "T"
                { "TCA", "TC" }, // Turks and Caicos Islands
                { "TCD", "TD" }, // Chad
                { "ATF", "TF" }, // French Southern Territories
                { "TGO", "TG" }, // Togo
                { "THA", "TH" }, // Thailand
                { "TJK", "TJ" }, // Tajikistan
                { "TKL", "TK" }, // Tokelau
                { "TKM", "TM" }, // Turkmenistan
                { "TON", "TO" }, // Tonga
                { "TLS", "TP" }, // Timor-Leste (historical "TP")
                { "TUR", "TR" }, // Turkey
                { "TTO", "TT" }, // Trinidad and Tobago
                { "TUV", "TV" }, // Tuvalu
                { "TWN", "TW" }, // Taiwan
                { "TZA", "TZ" }, // Tanzania
                { "UKR", "UA" }, // Ukraine
                { "UGA", "UG" }, // Uganda
                { "UMI", "UM" }, // United States Minor Outlying Islands
                { "USA", "US" }, // United States
                { "URY", "UY" }, // Uruguay
                { "UZB", "UZ" }, // Uzbekistan
                { "VXX", "V" }, // Custom code for single-letter "V"
                { "VAT", "VA" }, // Vatican City
                { "VCT", "VC" }, // Saint Vincent and the Grenadines
                { "VEN", "VE" }, // Venezuela
                { "VGB", "VG" }, // British Virgin Islands
                { "VIR", "VI" }, // Virgin Islands, U.S.
                { "VUT", "VU" }, // Vanuatu
                { "WLF", "WF" }, // Wallis and Futuna
                { "WSM", "WS" }, // Samoa
                { "XXX", "X" }, // Custom code for single-letter "X"
                { "XKO", "XK" }, // Kosovo (user-assigned)
                { "YEM", "YE" }, // Yemen
                { "MYT", "YT" }, // Mayotte
                { "YUG", "YU" }, // Yugoslavia (historical)
                { "ZAF", "ZA" }, // South Africa
                { "ZMB", "ZM" }, // Zambia
                { "ZAR", "ZR" }, // Zaire (historical, now "CD")
                { "ZWE", "ZW" }, // Zimbabwe
                { "ZXX", "ZX" } // Custom code for non-standard "ZX"
            };

        public const int ManagementAddressTypeCodeBulstat = 718;

        public static Dictionary<int, string> CountryCodeDictionaryBulstat = new Dictionary<int, string>
        {
            { 36, "AU" },
            { 40, "AT" },
            { 31, "AZ" },
            { 8, "AL" },
            { 12, "DZ" },
            { 24, "AO" },
            { 660, "AI" },
            { 20, "AD" },
            { 10, "AQ" },
            { 28, "AG" },
            { 32, "AR" },
            { 51, "AM" },
            { 533, "AW" },
            { 4, "AF" },
            { 50, "BD" },
            { 52, "BB" },
            { 44, "BS" },
            { 48, "BH" },
            { 998, "WC" },
            { 112, "BY" },
            { 56, "BE" },
            { 84, "BZ" },
            { 204, "BJ" },
            { 60, "BM" },
            { 68, "BO" },
            { 1, "BQ" },
            { 70, "BA" },
            { 72, "BW" },
            { 76, "BR" },
            { 86, "IO" },
            { 96, "BN" },
            { 74, "BV" },
            { 854, "BF" },
            { 108, "BI" },
            { 64, "BT" },
            { 100, "BG" },
            { 548, "VU" },
            { 336, "VA" },
            { 826, "GB" },
            { 862, "VE" },
            { 704, "VN" },
            { 92, "VG" },
            { 850, "VI" },
            { 266, "GA" },
            { 270, "GM" },
            { 288, "GH" },
            { 328, "GY" },
            { 312, "GP" },
            { 320, "GT" },
            { 324, "GN" },
            { 624, "GW" },
            { 276, "DE" },
            { 831, "GG" },
            { 292, "GI" },
            { 308, "GD" },
            { 304, "GL" },
            { 268, "GE" },
            { 316, "GU" },
            { 300, "GR" },
            { 208, "DK" },
            { 180, "CD" },
            { 262, "DJ" },
            { 832, "JE" },
            { 212, "DM" },
            { 214, "DO" },
            { 997, "DD" },
            { 818, "EG" },
            { 218, "EC" },
            { 226, "GQ" },
            { 232, "ER" },
            { 233, "EE" },
            { 231, "ET" },
            { 2, "ZR" },
            { 894, "ZM" },
            { 732, "EH" },
            { 716, "ZW" },
            { 376, "IL" },
            { 626, "TL" },
            { 16, "AS" },
            { 356, "IN" },
            { 360, "ID" },
            { 368, "IQ" },
            { 364, "IR" },
            { 372, "IE" },
            { 352, "IS" },
            { 724, "ES" },
            { 380, "IT" },
            { 887, "YE" },
            { 400, "JO" },
            { 132, "CV" },
            { 398, "KZ" },
            { 136, "KY" },
            { 116, "KH" },
            { 120, "CM" },
            { 124, "CA" },
            { 634, "QA" },
            { 404, "KE" },
            { 196, "CY" },
            { 417, "KG" },
            { 296, "KI" },
            { 156, "CN" },
            { 166, "CC" },
            { 170, "CO" },
            { 174, "KM" },
            { 178, "CG" },
            { 410, "KR" },
            { 408, "KP" },
            { 95, "XK" },
            { 188, "CR" },
            { 384, "CI" },
            { 192, "CU" },
            { 414, "KW" },
            { 184, "CK" },
            { 3, "CW" },
            { 418, "LA" },
            { 428, "LV" },
            { 426, "LS" },
            { 430, "LR" },
            { 434, "LY" },
            { 422, "LB" },
            { 440, "LT" },
            { 438, "LI" },
            { 442, "LU" },
            { 478, "MR" },
            { 480, "MU" },
            { 450, "MG" },
            { 175, "YT" },
            { 446, "MO" },
            { 807, "MK" },
            { 454, "MW" },
            { 458, "MY" },
            { 462, "MV" },
            { 466, "ML" },
            { 581, "UM" },
            { 470, "MT" },
            { 504, "MA" },
            { 474, "MQ" },
            { 584, "MH" },
            { 484, "MX" },
            { 104, "MM" },
            { 583, "FM" },
            { 508, "MZ" },
            { 498, "MD" },
            { 492, "MC" },
            { 496, "MN" },
            { 500, "MS" },
            { 516, "NA" },
            { 520, "NR" },
            { 524, "NP" },
            { 562, "NE" },
            { 566, "NG" },
            { 528, "NL" },
            { 530, "AN" },
            { 558, "NI" },
            { 570, "NU" },
            { 554, "NZ" },
            { 540, "NC" },
            { 578, "NO" },
            { 574, "NF" },
            { 784, "AE" },
            { 5, "AX" },
            { 512, "OM" },
            { 833, "IM" },
            { 586, "PK" },
            { 585, "PW" },
            { 275, "PS" },
            { 591, "PA" },
            { 598, "PG" },
            { 600, "PY" },
            { 604, "PE" },
            { 612, "PN" },
            { 616, "PL" },
            { 620, "PT" },
            { 630, "PR" },
            { 638, "RE" },
            { 162, "CX" },
            { 646, "RW" },
            { 642, "RO" },
            { 643, "RU" },
            { 222, "SV" },
            { 882, "WS" },
            { 674, "SM" },
            { 678, "ST" },
            { 682, "SA" },
            { 840, "US" },
            { 744, "SJ" },
            { 654, "SH" },
            { 580, "MP" },
            { 659, "KN" },
            { 670, "VC" },
            { 662, "LC" },
            { 690, "SC" },
            { 6, "BL" },
            { 7, "MF" },
            { 666, "PM" },
            { 686, "SN" },
            { 694, "SL" },
            { 702, "SG" },
            { 9, "SX" },
            { 760, "SY" },
            { 703, "SK" },
            { 705, "SI" },
            { 90, "SB" },
            { 706, "SO" },
            { 748, "SZ" },
            { 736, "SD" },
            { 740, "SR" },
            { 688, "RS" },
            { 891, "CS" },
            { 762, "TJ" },
            { 158, "TW" },
            { 764, "TH" },
            { 834, "TZ" },
            { 768, "TG" },
            { 772, "TK" },
            { 776, "TO" },
            { 780, "TT" },
            { 798, "TV" },
            { 788, "TN" },
            { 795, "TM" },
            { 792, "TR" },
            { 796, "TC" },
            { 800, "UG" },
            { 860, "UZ" },
            { 804, "UA" },
            { 348, "HU" },
            { 876, "WF" },
            { 858, "UY" },
            { 234, "FO" },
            { 242, "FJ" },
            { 608, "PH" },
            { 246, "FI" },
            { 238, "FK" },
            { 250, "FR" },
            { 249, "FX" },
            { 254, "GF" },
            { 258, "PF" },
            { 260, "TF" },
            { 332, "HT" },
            { 340, "HN" },
            { 344, "HK" },
            { 191, "HR" },
            { 334, "HM" },
            { 140, "CF" },
            { 148, "TD" },
            { 499, "ME" },
            { 203, "CZ" },
            { 152, "CL" },
            { 756, "CH" },
            { 752, "SE" },
            { 144, "LK" },
            { 11, "SS" },
            { 710, "ZA" },
            { 239, "GS" },
            { 388, "JM" },
            { 392, "JP" }
        };
    }
}