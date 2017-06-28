using IMEVENT.Data;
using IMEVENT.SharedEnums;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NLog;

namespace IMEVENT.Services
{
    public partial class ExcelDataExtractor : IDataExtractor
    {
        private static Logger log = LogManager.GetCurrentClassLogger();


        public ApplicationDbContext DBcontext { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
        // for now we are going to stop the processing of the file if we see 5 consecutive empty lines.
        private static int MAX_EMPTY_CELLS = 5;
        private static readonly string COLUMN_LASTNAME = "B";
        private static readonly string COLUMN_FIRSTNAME = "C";
        private static readonly string COLUMN_SEX = "D";
        private static readonly string COLUMN_RETREAT = "E";
        private static readonly string COLUMN_TOWN = "F";
        private static readonly string COLUMN_GROUP = "G";
        private static readonly string COLUMN_LEVEL = "H";
        private static readonly string COLUMN_INVITED_BY = "I";
        private static readonly string COLUMN_SHARING_CATEGORY = "J";
        private static readonly string COLUMN_LANGUAGE = "K";
        private static readonly string COLUMN_AMOUNTPAID = "L";
        private static readonly string COLUMN_EMAIL = "M";
        private static readonly string COLUMN_PHONE = "N";
        private static readonly string COLUMN_RESPONSIBLE = "O";
        private static readonly string COLUMN_REMARKS = "P";        
        private static readonly string COLUMN_PRECISION = "Q";
        private static readonly string COLUMN_ZONE = "R";
        private static readonly string COLUMN_SOUS_ZONE = "S";
        private static readonly string COLUMN_ORIGIN_TOWN = "T";
        private static readonly string COLUMN_ORIGIN_GROUP = "U";
        private static readonly string COLUMN_HALL_TYPE = "V";
        private static readonly string COLUMN_TABLE_TYPE = "W";
        private static readonly string COLUMN_DORM_TYPE = "X";        

        private static readonly int USER_WORKSHEET_INDEX = 1;
        private static readonly int REF_WORKSHEET_INDEX = 2;
        private static readonly int HALL_WORKSHEET_INDEX = 3;
        private static readonly int DORMS_WORKSHEET_INDEX = 4;
        private static readonly int SHARING_GROUP_WORKSHEET_INDEX = 5;

        private static readonly string COLUMN_NAME = "A";
        private static readonly string COLUMN_CAPACITE = "B";

        private static readonly string REFECTORY_NAME = "A";
        private static readonly string TABLE_NAME = "B";
        private static readonly string TABLE_CAPACITY = "C";
        private static readonly string TYPE_TABLE = "D";
        private static readonly string TYPE_DORM = "C";
        private static readonly string CATEGORY_DORM = "D";

        private static readonly string TYPE_HALL = "C";

        //Sharing group        
        private static readonly string TYPE_SG = "A";
        private static readonly string TYPE_SG_CAPACITY = "B";

        public void ExtractDataFromSource(string source, int EventId)
        {
            this.Source = source;
            FileInfo existingFile = new FileInfo(source);
            if (!existingFile.Exists)
            {
                return;
            }

            using (ExcelPackage package = new ExcelPackage(existingFile))
            {
                ExcelWorksheet userWorksheet = package.Workbook.Worksheets[USER_WORKSHEET_INDEX];
                LoadUsers(userWorksheet, EventId);
                ExcelWorksheet dormWorksheet = package.Workbook.Worksheets[DORMS_WORKSHEET_INDEX];
                LoadDorms(dormWorksheet, EventId);
                ExcelWorksheet hallWorksheet = package.Workbook.Worksheets[HALL_WORKSHEET_INDEX];
                LoadHalls(hallWorksheet, EventId);
                ExcelWorksheet refectoryWorksheet = package.Workbook.Worksheets[REF_WORKSHEET_INDEX];
                LoadRefectories(refectoryWorksheet, EventId);
                ExcelWorksheet sharingGroupWorksheet = package.Workbook.Worksheets[SHARING_GROUP_WORKSHEET_INDEX];
                LoadSharingGroups(sharingGroupWorksheet, EventId);
            }
        }

        public void LoadUsers(ExcelWorksheet worksheet, int EventId)
        {
            int maxEmpty = 0;
            int currentRow = 2;
            while (maxEmpty < MAX_EMPTY_CELLS)
            {
                EventAttendee attendee = new EventAttendee();
                string name = (string)worksheet.Cells[COLUMN_FIRSTNAME + Convert.ToString(currentRow)].Value;
                if (!string.IsNullOrEmpty(name))
                {
                    User u = GetUserFromSpreadSheet(currentRow, worksheet);
                    attendee.EventId = EventId;
                    attendee.UserId = u.Id;
                    try
                    {
                        attendee.TableType = Convertors.GetRegimeType((string)worksheet.Cells[COLUMN_TABLE_TYPE + Convert.ToString(currentRow)].Value);
                    }
                    catch
                    {
                        log.Info("Regime import for table failed, setting regime to none");
                        attendee.TableType = RegimeEnum.NONE;
                    }
                    try
                    {
                        attendee.SectionType = Convertors.GetHallSectionType((string)worksheet.Cells[COLUMN_HALL_TYPE + Convert.ToString(currentRow)].Value);
                    }
                    catch
                    {
                        attendee.SectionType = HallSectionTypeEnum.NONE;
                    }
                    try
                    {
                        attendee.DormCategory = Convertors.GetDormirtoryCategory((string)worksheet.Cells[COLUMN_DORM_TYPE + Convert.ToString(currentRow)].Value);
                    }
                    catch
                    {
                        attendee.SectionType = HallSectionTypeEnum.NONE;
                    }

                    object retr = worksheet.Cells[COLUMN_RETREAT + Convert.ToString(currentRow)].Value;
                    attendee.Retreats = retr == null ? "" : retr.ToString();

                    //The output file separator is ","; this character should be therefore avoided in comment
                    string remark = (string)worksheet.Cells[COLUMN_REMARKS + Convert.ToString(currentRow)].Value;                    
                    attendee.Remarks = (string.IsNullOrEmpty(remark)) ? "" : remark.Replace(",",";");

                    string precision = (string)worksheet.Cells[COLUMN_PRECISION + Convert.ToString(currentRow)].Value;                    
                    attendee.Precision = (string.IsNullOrEmpty(precision)) ? "" : precision.Replace(",", ";");

                    try
                    {
                        attendee.AmountPaid = Convert.ToInt32(worksheet.Cells[COLUMN_AMOUNTPAID + Convert.ToString(currentRow)].Value);
                    }
                    catch (Exception)
                    {
                        attendee.AmountPaid = 0;
                    }

                    attendee.InvitedBy = User.GetUserIdByName((string)worksheet.Cells[COLUMN_INVITED_BY + Convert.ToString(currentRow)].Value);                    

                    try
                    {
                        attendee.SharingCategory = Convertors.GetSharingGroupCategory((string)worksheet.Cells[COLUMN_SHARING_CATEGORY + Convert.ToString(currentRow)].Value);
                    }
                    catch
                    {
                        attendee.SharingCategory = SharingGroupCategoryEnum.ADULTE_SINGLE;
                    }

                    attendee.Persist();
                    maxEmpty = 0;
                }
                else
                {
                    maxEmpty++;
                }
                currentRow++;
            }
        }

        public void LoadHalls(ExcelWorksheet worksheet, int EventId)
        {
            int maxEmpty = 0;
            int currentRow = 2;
            while (maxEmpty < MAX_EMPTY_CELLS)
            {
                string name = (string)worksheet.Cells[COLUMN_NAME + Convert.ToString(currentRow)].Value;
                if (!String.IsNullOrEmpty(name))
                {
                    GetHallsFromSpreadSheet(currentRow, worksheet, EventId);

                    maxEmpty = 0;
                }
                else
                {
                    maxEmpty++;
                }
                currentRow++;
            }
        }

        private void GetHallsFromSpreadSheet(int row, ExcelWorksheet sheet, int EventId)
        {
            Hall h = new Hall();
            h.Capacity = Convert.ToInt32(sheet.Cells[COLUMN_CAPACITE + Convert.ToString(row)].Value);
            h.Name = (string)sheet.Cells[COLUMN_NAME + Convert.ToString(row)].Value;
            h.EventId = EventId;
            try
            {
                h.HallType = Convertors.GetHallSectionType((string)sheet.Cells[TYPE_HALL + Convert.ToString(row)].Value);
            }
            catch (Exception)
            {
                h.HallType = HallSectionTypeEnum.NONE;
            }
            h.Persist();
        }

        public void LoadRefectories(ExcelWorksheet worksheet, int EventId)
        {
            int maxEmpty = 0;
            int currentRow = 2;
            while (maxEmpty < MAX_EMPTY_CELLS)
            {
                string name = (string)worksheet.Cells[COLUMN_NAME + Convert.ToString(currentRow)].Value;
                if (!String.IsNullOrEmpty(name))
                {
                    GetRefectoryFromSpreadSheet(currentRow, worksheet, EventId);
                    maxEmpty = 0;
                }
                else
                {
                    maxEmpty++;
                }
                currentRow++;
            }
        }

        private void GetRefectoryFromSpreadSheet(int row, ExcelWorksheet sheet, int EventId)
        {
            Refectory h = new Refectory();

            h.Name = (string)sheet.Cells[REFECTORY_NAME + Convert.ToString(row)].Value;
            h.EventId = EventId;
            h.Persist();
            Table t = new Table();
            t.Name = (string)sheet.Cells[TABLE_NAME + Convert.ToString(row)].Value;
            t.RefectoryId = h.Id;
            try
            {
                t.Capacity = Convert.ToInt32(sheet.Cells[TABLE_CAPACITY + Convert.ToString(row)].Value);
            }
            catch (Exception)
            {
                t.Capacity = 0;
            }

            try
            {
                t.RegimeType = Convertors.GetRegimeType((string)sheet.Cells[TYPE_TABLE + Convert.ToString(row)].Value.ToString().ToLowerInvariant());               
            }
            catch (Exception)
            {
                t.RegimeType = RegimeEnum.NONE;
            }
            t.Persist();            
        }

        public void LoadDorms(ExcelWorksheet worksheet, int EventId)
        {
            int maxEmpty = 0;
            int currentRow = 2;
            while (maxEmpty < MAX_EMPTY_CELLS)
            {
                string name = (string)worksheet.Cells[COLUMN_NAME + Convert.ToString(currentRow)].Value;
                if (!String.IsNullOrEmpty(name))
                {
                    GetDormFromSpreadSheet(currentRow, worksheet, EventId);
                   
                    maxEmpty = 0;
                }
                else
                {
                    maxEmpty++;
                }
                currentRow++;
            }
        }

        private void GetDormFromSpreadSheet(int row, ExcelWorksheet sheet, int EventId)
        {
            Dormitory h = new Dormitory();
            h.Capacity = Convert.ToInt32(sheet.Cells[COLUMN_CAPACITE + Convert.ToString(row)].Value);
            h.Name = (string)sheet.Cells[COLUMN_NAME + Convert.ToString(row)].Value;
            h.EventId = EventId;

            try
            {
                h.DormType = Convertors.GetDormirtoryType((string)sheet.Cells[TYPE_DORM + Convert.ToString(row)].Value.ToString().ToLowerInvariant());
            }
            catch (Exception)
            {
                h.DormType = DormitoryTypeEnum.NONE;
            }

            try
            {
                h.DormCategory = Convertors.GetDormirtoryCategory((string)sheet.Cells[CATEGORY_DORM + Convert.ToString(row)].Value.ToString().ToLowerInvariant());
            }
            catch (Exception)
            {
                h.DormCategory = DormitoryCategoryEnum.MATELAS;
            }

            h.Persist();
        }

        private User GetUserFromSpreadSheet(int row, ExcelWorksheet sheet)
        {
            User user = new User();
            user.FirstName = (string)sheet.Cells[COLUMN_FIRSTNAME + Convert.ToString(row)].Value;
            user.LastName = (string)sheet.Cells[COLUMN_LASTNAME + Convert.ToString(row)].Value;
            user.Sex = (string)sheet.Cells[COLUMN_SEX + Convert.ToString(row)].Value;
            user.Level = Convertors.GetMembershipLevel((string)sheet.Cells[COLUMN_LEVEL + Convert.ToString(row)].Value);
            user.Language = (string)sheet.Cells[COLUMN_LANGUAGE + Convert.ToString(row)].Value;
            user.Email = (string)sheet.Cells[COLUMN_EMAIL + Convert.ToString(row)].Value; 

            if (sheet.Cells[COLUMN_PHONE + Convert.ToString(row)].Value != null)
            {
                string val;
                try
                {
                    val = (string)sheet.Cells[COLUMN_PHONE + Convert.ToString(row)].Value;
                }
                catch (InvalidCastException)
                {
                    try
                    {
                        val = Convert.ToString((string)sheet.Cells[COLUMN_PHONE + Convert.ToString(row)].Value);
                    }
                    catch (Exception)
                    {
                        val = "";
                    }
                }

                user.PhoneNumber = val;
            }

            bool isResponsible = false;
            if (sheet.Cells[COLUMN_RESPONSIBLE + Convert.ToString(row)].Value == null)
            {
                isResponsible = false;
            }
            else
            {
                string strValue = sheet.Cells[COLUMN_RESPONSIBLE + Convert.ToString(row)].Value.ToString().ToLowerInvariant();
                isResponsible = strValue.Equals("oui") ? true : false;
            }
            user.IsGroupResponsible = isResponsible;
            user.Town = (string)sheet.Cells[COLUMN_TOWN + Convert.ToString(row)].Value;
            // read zone if it is not availlable create it,
            Zone zone = new Zone();
            zone.Label = (string)sheet.Cells[COLUMN_ZONE + Convert.ToString(row)].Value;
            zone.Id = Zone.GetIdRefectoryIdByName(DBcontext, zone.Label);
            zone.Id = zone.Persist();
            user.ZoneId = zone.Id;

            SousZone sousZone = new SousZone();
            sousZone.Label = (string)sheet.Cells[COLUMN_SOUS_ZONE + Convert.ToString(row)].Value;
            sousZone.ZoneId = zone.Id;
            sousZone.Id = sousZone.Persist();
            user.SousZoneId = sousZone.Id;

            Group group = new Group();
            group.ZoneId = zone.Id;
            group.SousZoneId = sousZone.Id;
            group.Label = (string)sheet.Cells[COLUMN_GROUP + Convert.ToString(row)].Value;
            group.Id = group.Persist();
            user.GroupId = group.Id;
            
            user.TownOriginLabel = (string)sheet.Cells[COLUMN_ORIGIN_TOWN + Convert.ToString(row)].Value;
            user.GroupOriginLabel = (string)sheet.Cells[COLUMN_ORIGIN_GROUP + Convert.ToString(row)].Value;

            user.persist();
            return user;
        }

        public void LoadSharingGroups(ExcelWorksheet worksheet, int EventId)
        {
            int maxEmpty = 0;
            int currentRow = 2;
            while (maxEmpty < MAX_EMPTY_CELLS)
            {
                string name = (string)worksheet.Cells[COLUMN_NAME + Convert.ToString(currentRow)].Value;
                if (!string.IsNullOrEmpty(name))
                {
                    GetSharingGroupsFromSpreadSheet(currentRow, worksheet, EventId);
                    maxEmpty = 0;
                }
                else
                {
                    maxEmpty++;
                }
                currentRow++;
            }
        }

        private void GetSharingGroupsFromSpreadSheet(int row, ExcelWorksheet sheet, int EventId)
        {
            SharingGroup sg = new SharingGroup();
            sg.Capacity = Convert.ToInt32(sheet.Cells[TYPE_SG_CAPACITY + Convert.ToString(row)].Value);            
            sg.EventId = EventId;
            try
            {
                sg.Type = Convertors.GetSharingGroupCategory((string)sheet.Cells[TYPE_SG + Convert.ToString(row)].Value);
            }
            catch (Exception)
            {
                sg.Type = SharingGroupCategoryEnum.ADULTE_SINGLE;
            }
            sg.Persist();
        }
    }
}