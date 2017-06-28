using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IMEVENT.Data;
using NLog;

namespace IMEVENT.SharedEnums
{
    public static class Convertors
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        public static string EventTypeToString(this EventTypeEnum evt, bool forFilename)
        {
            switch (evt)
            {
                case EventTypeEnum.RETRAITE_CAREME:
                    return forFilename ? "Retraite_Careme" : "Retraite de Careme";
                case EventTypeEnum.RETRAITE_DE_COUPLE:
                    return forFilename ? "Retraite_Couples" : "Retraite des Couple";
                case EventTypeEnum.RETRAITE_MEMBRE_ACTIF:
                    return forFilename ? "Retraite_Membre_Actif" : "Retraite des Membres Actifs";
                case EventTypeEnum.RETRAITE_RESPONSABLE:
                    return forFilename ? "Retraite_Responsable" : "Retraite des Responsables";
                case EventTypeEnum.RETRAITE_SINGLE:
                    return forFilename ? "Retraite_Single" : "Retraite Personne Single";
                case EventTypeEnum.GRANDE_RETRAITE:
                    return forFilename ? "Grande_Retraite" : "Grande Retraite";
                default:
                    log.Error(string.Format("EventTypeToString: unknown Event Type={0}"
                        , evt));
                    throw new System.NullReferenceException(string.Format("EventTypeToString: unknown Event Type={0}"
                        , evt));
            }
        }

        public static string MemberShipLevelToString(this MembershipLevelEnum level)
        {
            switch (level)
            {                
                case MembershipLevelEnum.SIMPLE:
                    return "Simple";
                case MembershipLevelEnum.REGULAR:
                    return "Régulier";
                case MembershipLevelEnum.AKTIV_1:
                    return "Actif 1";
                case MembershipLevelEnum.AKTIV_2:
                    return "Actif 2";
                case MembershipLevelEnum.AKTIV_3:
                    return "Actif 3";
                case MembershipLevelEnum.YOUNG_MODEL:
                    return "Jeune Phare";                
                case MembershipLevelEnum.AEF:
                    return "AEF";
                case MembershipLevelEnum.CANDIDATE_FULL_MEMBER:
                    return "CMP";
                case MembershipLevelEnum.FULL_MEMBER:
                    return "MP";
                case MembershipLevelEnum.INCARNATOR:
                    return "Inc.";
                case MembershipLevelEnum.RG:
                    return "RG";
                case MembershipLevelEnum.INVITED:
                    return "Invité";
                default:
                    log.Error(string.Format("MemberShipLevelToString: unknown MembershipLevelEnum={0}"
                        , level));
                    throw new System.NullReferenceException(string.Format("MemberShipLevelToString: unknown MembershipLevelEnum={0}"
                        , level));
            }
        }

        public static MembershipLevelEnum GetMembershipLevel(string type)
        {
            string tmp = type.Replace(" ", string.Empty).ToLower();
            switch (tmp)
            {
                case "simple":
                    return MembershipLevelEnum.SIMPLE;
                case "régulier":
                    return MembershipLevelEnum.REGULAR;
                case "actif1":
                    return MembershipLevelEnum.AKTIV_1;
                case "actif2":
                    return MembershipLevelEnum.AKTIV_2;
                case "actif3":
                    return MembershipLevelEnum.AKTIV_3;
                case "jeunephare":
                    return MembershipLevelEnum.YOUNG_MODEL;
                case "aef":
                    return MembershipLevelEnum.AEF;
                case "cmp":
                    return MembershipLevelEnum.CANDIDATE_FULL_MEMBER;
                case "mp":
                    return MembershipLevelEnum.FULL_MEMBER;
                case "inc.":
                case "inc":
                    return MembershipLevelEnum.INCARNATOR;
                case "rg":
                    return MembershipLevelEnum.RG;
                case "invité":
                    return MembershipLevelEnum.INVITED;
                default:
                    log.Error(string.Format("GetMembershipLevel: unknown MembershipLevelEnum={0}; input text={1}"
                        , tmp, type));
                    throw new System.NullReferenceException(string.Format("GetMembershipLevel: unknown MembershipLevelEnum={0}; input text={1}"
                        , tmp, type));                    
            }
        }

        public static string DormitoryCategoryToString(this DormitoryCategoryEnum type)
        {
            switch (type)
            {                
                case DormitoryCategoryEnum.MATELAS:
                    return "Matelas";
                case DormitoryCategoryEnum.BED:
                    return "Lit";
                case DormitoryCategoryEnum.BED_E:
                    return "Lit_E";
                case DormitoryCategoryEnum.BED_R:
                    return "Lit_R";
                case DormitoryCategoryEnum.VIP:
                    return "VIP";                
                default:
                    log.Error(string.Format("DormitoryCategoryToString: unknown DormitoryCategoryEnum={0}"
                        , type));
                    throw new System.NullReferenceException(string.Format("DormitoryCategoryToString: unknown DormitoryCategoryEnum={0}"
                        , type));                    
            }
        }

        public static string DormitoryTypeToString(this DormitoryTypeEnum type)
        {
            string ret = "Dortoir ";
            switch (type)
            {
                case DormitoryTypeEnum.MEN:
                    return ret + "Homme";
                case DormitoryTypeEnum.WOMEN:
                    return ret + "Femme";
                case DormitoryTypeEnum.NONE:
                    return ret + "Commun";
                default:
                    log.Error(string.Format("DormitoryTypeToString: unknown DormitoryTypeEnum={0}"
                        , type));
                    throw new System.NullReferenceException(string.Format("DormitoryTypeToString: unknown DormitoryTypeEnum={0}"
                        , type));
            }
        }

        public static DormitoryCategoryEnum GetDormirtoryCategory(string type)
        {
            string tmp = type.Replace(" ", string.Empty).ToLower();
            switch (tmp)
            {
                case "vip":
                    return DormitoryCategoryEnum.VIP;
                case "lit":
                    return DormitoryCategoryEnum.BED;
                case "lit_e":
                    return DormitoryCategoryEnum.BED_E;
                case "lit_r":
                    return DormitoryCategoryEnum.BED_R;
                case "matelas":
                    return DormitoryCategoryEnum.MATELAS;
                default:
                    log.Error(string.Format("GetDormirtoryCategory: unknown DormitoryCategoryEnum={0}; input={1}"
                        , tmp, type));
                    throw new System.NullReferenceException(string.Format("GetDormirtoryCategory: unknown DormitoryCategoryEnum={0}; input={1}"
                        , tmp, type));
            }
        }

        public static DormitoryTypeEnum GetDormirtoryType(string type)
        {
            string tmp = type.Replace(" ", string.Empty).ToLower();
            switch (tmp)
            {
                case "femme":
                    return DormitoryTypeEnum.WOMEN;
                case "homme":
                    return DormitoryTypeEnum.MEN;
                case "commun":
                    return DormitoryTypeEnum.NONE;
                default:
                    log.Error(string.Format("GetDormirtoryType: unknown DormitoryTypeEnum={0}; input={1}"
                        , tmp, type));
                    throw new System.NullReferenceException(string.Format("GetDormirtoryType: unknown DormitoryTypeEnum={0}; input={1}"
                        , tmp, type));
            }
        }

        public static string SharingGroupCategoryToString(this SharingGroupCategoryEnum type)
        {
            switch (type)
            {                
                case SharingGroupCategoryEnum.UNIVERSITAIRE_DEBUTANT:
                    return "Univers Debt";
                case SharingGroupCategoryEnum.UNIVERSITAIRE_MAJEUR:
                    return "Univers Maj";                
                case SharingGroupCategoryEnum.JEUNE_TRAVAILLEUR_MAJEUR:
                    return "Jeune Trav Maj";
                case SharingGroupCategoryEnum.JEUNE_TRAVAILLEUR:
                    return "Jeune Trav";
                case SharingGroupCategoryEnum.JEUNE_TRAVAILLEUR_SENIOR:
                    return "Jeune Trav Sen";
                case SharingGroupCategoryEnum.SECOND_INTERMEDIARE:
                    return "Second Interm";
                case SharingGroupCategoryEnum.SECOND_JUNIOR:
                    return "Second Junior";
                case SharingGroupCategoryEnum.ADULTE_SINGLE:                    
                case SharingGroupCategoryEnum.ADULTE_MARIE:                   
                case SharingGroupCategoryEnum.JEUNE_MARIE:                                   
                    return "Adulte";
                default:
                    log.Error(string.Format("SharingGroupCategoryToString: unknown SharingGroupCategoryEnum={0}"
                        , type));
                    throw new System.NullReferenceException(string.Format("SharingGroupCategoryToString: unknown SharingGroupCategoryEnum={0}"
                        , type));
            }
        }        

        public static SharingGroupCategoryEnum GetSharingGroupCategory(string type)
        {
            string tmp = type.ToLower().Replace(" ", string.Empty);
            switch (tmp)
            {
                case "adultes":
                    return SharingGroupCategoryEnum.ADULTE_SINGLE;
                case "adultem":
                    return SharingGroupCategoryEnum.ADULTE_MARIE;
                case "universdebt":
                    return SharingGroupCategoryEnum.UNIVERSITAIRE_DEBUTANT;
                case "universmaj":
                    return SharingGroupCategoryEnum.UNIVERSITAIRE_MAJEUR;
                case "jeunem":
                    return SharingGroupCategoryEnum.JEUNE_MARIE;
                case "jeunetravmaj":
                    return SharingGroupCategoryEnum.JEUNE_TRAVAILLEUR_MAJEUR;
                case "jeunetrav":
                    return SharingGroupCategoryEnum.JEUNE_TRAVAILLEUR;
                case "jeunetravsen":
                    return SharingGroupCategoryEnum.JEUNE_TRAVAILLEUR_SENIOR;
                case "secondinterm":
                    return SharingGroupCategoryEnum.SECOND_INTERMEDIARE;
                case "secondjunior":
                    return SharingGroupCategoryEnum.SECOND_JUNIOR;                
                default:
                    log.Error(string.Format("GetSharingGroupCategory: unknown SharingGroupCategoryEnum={0}; input={1}"
                        , tmp, type));
                    throw new System.NullReferenceException(string.Format("GetSharingGroupCategory: unknown SharingGroupCategoryEnum={0}; input={1}"
                        , tmp, type));                    
            }
        }

        public static string RegimeToString(this RegimeEnum type)
        {
            switch (type)
            {
                case RegimeEnum.CLERICAL:
                    return "Clergé";
                case RegimeEnum.COOKING:
                    return "Cuisine";
                case RegimeEnum.DISABLED:
                    return "Handicapé";
                case RegimeEnum.FULL_MEMBER:
                    return "MP";
                case RegimeEnum.CANDIDATE_FULL_MEMBER:
                    return "CMP";
                case RegimeEnum.GENERAL_MANAGER:
                    return "Responsable Général";
                case RegimeEnum.HEALTH_SERVICE:
                    return "Service Santé";
                case RegimeEnum.MUSIC_INSTRUMENT_SERVICE:
                    return "Service Instrument";
                case RegimeEnum.NEW_BORN:
                    return "Nouveaux-né";
                case RegimeEnum.RELIGIOUS:
                    return "Réligieux";
                case RegimeEnum.SECOND_LANGUAGE:
                    return "Seconde Langue";
                case RegimeEnum.SONG_SERVICE:
                    return "Service Chant";
                case RegimeEnum.SPECIAL_GUEST:
                    return "Invite Spécial";
                case RegimeEnum.TRANSLATION_SERVICE:
                    return "Service Traduction";
                case RegimeEnum.UNDER_DIET:
                    return "Sous Régime";
                case RegimeEnum.AEF:
                    return "AEF";
                case RegimeEnum.NONE:
                    return "Sans Régime";
                default:
                    log.Error(string.Format("RegimeToString: unknown RegimeEnum={0}"
                        , type));
                    throw new System.NullReferenceException(string.Format("RegimeToString: unknown RegimeEnum={0}"
                        , type));                    
            }
        }

        public static RegimeEnum GetRegimeType(string type)
        {
            string tmp = type.Replace(" ", string.Empty).ToLower();
            switch (tmp)
            {                
                case "sousrégime":
                    return RegimeEnum.UNDER_DIET;
                case "invitéspécial":
                    return RegimeEnum.SPECIAL_GUEST;
                case "mp":
                    return RegimeEnum.FULL_MEMBER;
                case "cmp":
                    return RegimeEnum.CANDIDATE_FULL_MEMBER;
                case "handicapé":
                    return RegimeEnum.DISABLED;
                case "clergé":
                    return RegimeEnum.CLERICAL;
                case "santé":
                    return RegimeEnum.HEALTH_SERVICE;
                case "nouveau-né":
                    return RegimeEnum.NEW_BORN;
                case "cuisine":
                    return RegimeEnum.COOKING;
                case "rg":
                    return RegimeEnum.GENERAL_MANAGER;
                case "secondelangue":
                    return RegimeEnum.SECOND_LANGUAGE;
                case "servicechant":
                    return RegimeEnum.SONG_SERVICE;
                case "servicetraduction":
                    return RegimeEnum.TRANSLATION_SERVICE;
                case "serviceinstrument":
                    return RegimeEnum.MUSIC_INSTRUMENT_SERVICE;
                case "religieux":
                    return RegimeEnum.RELIGIOUS;
                case "aef":
                    return RegimeEnum.AEF;
                case "aucun":
                    return RegimeEnum.NONE;
                default:
                    log.Error(string.Format("GetRegimeType: unknown RegimeEnum={0}; input={1}"
                        , tmp, type));
                    throw new System.NullReferenceException(string.Format("GetRegimeType: unknown RegimeEnum={0}; input={1}"
                        , tmp, type));                                   
            }
        }

        public static string HallSectionTypeToString(HallSectionTypeEnum type)
        {
            switch (type)
            {
                case HallSectionTypeEnum.CLERICAL:
                    return "Clergé";
                case HallSectionTypeEnum.COOKING:
                    return "Cuisine";
                case HallSectionTypeEnum.DISABLED:
                    return "Handicapé";
                case HallSectionTypeEnum.FULL_MEMBER:
                    return "MP";
                case HallSectionTypeEnum.CANDIDATE_FULL_MEMBER:
                    return "CMP";
                case HallSectionTypeEnum.GENERAL_MANAGER:
                    return "RG";
                case HallSectionTypeEnum.HEALTH_SERVICE:
                    return "Service Santé";
                case HallSectionTypeEnum.MUSIC_INSTRUMENT_SERVICE:
                    return "Service Instrument";
                case HallSectionTypeEnum.NEW_BORN:
                    return "Nouveaux-né";
                case HallSectionTypeEnum.RELIGIOUS:
                    return "Religieux";
                case HallSectionTypeEnum.SECOND_LANGUAGE:
                    return "Seconde Langue";
                case HallSectionTypeEnum.SONG_SERVICE:
                    return "Service Chant";
                case HallSectionTypeEnum.SPECIAL_GUEST:
                    return "Invité Spécial";
                case HallSectionTypeEnum.TRANSLATION_SERVICE:
                    return "Service Traduction";
                case HallSectionTypeEnum.MASS_REQUEST:
                    return "Demande Messe";
                case HallSectionTypeEnum.AEF:
                    return "AEF";
                case HallSectionTypeEnum.NONE:
                    return "Public";
                default:
                    log.Error(string.Format("HallSectionTypeToString: unknown HallSectionTypeEnum={0}"
                        , type));
                    throw new System.NullReferenceException(string.Format("HallSectionTypeToString: unknown HallSectionTypeEnum={0}"
                        , type));                    
            }
        }

        public static HallSectionTypeEnum GetHallSectionType(string type)
        {
            string tmp = type.Replace(" ", string.Empty).ToLower();
            switch (tmp)
            {                
                case "rg":
                    return HallSectionTypeEnum.GENERAL_MANAGER;
                case "servicechant":
                    return HallSectionTypeEnum.SONG_SERVICE;
                case "serviceinstrument":
                    return HallSectionTypeEnum.MUSIC_INSTRUMENT_SERVICE;
                case "servicetraduction":
                    return HallSectionTypeEnum.TRANSLATION_SERVICE;
                case "invitéspécial":
                    return HallSectionTypeEnum.SPECIAL_GUEST;
                case "secondelangue":
                    return HallSectionTypeEnum.SECOND_LANGUAGE;
                case "mp":
                    return HallSectionTypeEnum.FULL_MEMBER;
                case "cmp":
                    return HallSectionTypeEnum.CANDIDATE_FULL_MEMBER;
                case "handicapé":
                    return HallSectionTypeEnum.DISABLED;
                case "clergé":
                    return HallSectionTypeEnum.CLERICAL;
                case "santé":
                    return HallSectionTypeEnum.HEALTH_SERVICE;
                case "nouveau-né":
                    return HallSectionTypeEnum.NEW_BORN;
                case "cuisine":
                    return HallSectionTypeEnum.COOKING;
                case "religieux":
                    return HallSectionTypeEnum.RELIGIOUS;
                case "demandemesse":
                    return HallSectionTypeEnum.MASS_REQUEST;
                case "aef":
                    return HallSectionTypeEnum.AEF;
                case "aucun":
                    return HallSectionTypeEnum.NONE;
                default:
                    log.Error(string.Format("GetHallSectionType: unknown HallSectionTypeEnum={0}; input={1}"
                        , tmp, type));
                    throw new System.NullReferenceException(string.Format("GetHallSectionType: unknown HallSectionTypeEnum={0}; input={1}"
                        , tmp, type));
            }
        }
    }
}
