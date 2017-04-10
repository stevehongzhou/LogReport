using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Sql;

namespace CAOLogMonitor
{
    class Program
    {
        static void Main()
        {

            try
            {
                DataTable tbl = new DataTable();
                string[] lines = System.IO.File.ReadAllLines(@"C:\CAOLOG.txt");
                int n = 0;
                string User = "";
                string CID = "";
                string Step = "";
                string StepTime = "";
                string SpanTime = "";
                string Date = "";
                int len;
                string ICDCode = "";

                DataTable dt_temp = new DataTable();
                dt_temp.Columns.Add(new DataColumn("Date"));
                dt_temp.Columns.Add(new DataColumn("LineNum"));
                dt_temp.Columns.Add(new DataColumn("ID"));
                dt_temp.Columns.Add(new DataColumn("User"));
                dt_temp.Columns.Add(new DataColumn("CID"));
                dt_temp.Columns.Add(new DataColumn("StepDesc"));
                dt_temp.Columns.Add(new DataColumn("ICDCode"));
                dt_temp.Columns.Add(new DataColumn("Time"));
                dt_temp.Columns.Add(new DataColumn("TimeSpan"));

                foreach (string line in lines)
                {
                    n++;
                    //Date:
                    if (line.IndexOf("2017-") < 0) continue;
                    Date = line.Substring(0, 19);
                    //ID:
                    if (line.IndexOf(") ID:") < 0) continue;
                    string ID = line.Substring(line.IndexOf(") ID:") + 2, 24);
                    string LineNum = n.ToString();
                    //User:
                    if ((line.IndexOf(" - WebDE_PIA_PIADataEntry:LogDetails") >= 0) && (line.IndexOf("[(null)] - WebDE_PIA_PIADataEntry") < 0))
                    {
                        len = line.IndexOf(" - WebDE_PIA_PIADataEntry:LogDetails") - (line.IndexOf(" [(null)] - ") + 12);
                        if (len > 0)
                            User = line.Substring(line.IndexOf(" [(null)] - ") + 12, len);
                    }
                    else if ((line.IndexOf(" - Outcomes.Core.Global.Logger") >= 0) && (line.IndexOf("[(null)] - Outcomes.Core.Global.Logger") < 0))
                        User = line.Substring(line.IndexOf(" [(null)] - ") + 12, line.IndexOf(" - Outcomes.Core.Global.Logger") - (line.IndexOf(" [(null)] - ") + 12));
                    else
                        User = "";
                    //CID:
                    if (line.IndexOf("CID: ") >= 0)
                        CID = line.Substring(line.IndexOf("CID: ") + 5, (line.IndexOf(", RowIndex:") - line.IndexOf("CID: ") - 5));
                    else
                        CID = "";

                    //Pre-Client Side Validation

                    if (line.IndexOf("Pre-Client Side Validation") > 0)
                    {
                        SpanTime = line.Substring(line.IndexOf("TimeSpan:") + 9, line.IndexOf(" ms (CID:") - (line.IndexOf("TimeSpan:") + 9));
                        StepTime = "";
                        Step = "Pre-Client Side Validation";
                    }
                    //Validate FCodes Total Time
                    else if (line.IndexOf("Validate FCodes Total Time") > 0)
                    {
                        SpanTime = line.Substring(line.IndexOf("Validate FCodes Total Time - ") + 29, line.IndexOf(" ms (CID:") - (line.IndexOf("Validate FCodes Total Time - ") + 29));
                        StepTime = "";
                        Step = "Validate FCodes Total Time";
                    }

                    //Send Server Request
                    else if (line.IndexOf("Send Server Request") > 0)
                    {
                        StepTime = line.Substring(line.IndexOf("Client Machine Out Time:") + 24, 12);
                        SpanTime = "";
                        Step = "Send Server Request";
                    }
                    //Server Cache  
                    else if (line.IndexOf("Server Cache") > 0)
                    {
                        SpanTime = line.Substring(line.IndexOf("TimeSpan:") + 9, line.IndexOf(" ms (CID:") - (line.IndexOf("TimeSpan:") + 9));
                        StepTime = "";
                        Step = "Server Cache";
                    }
                    //Received Server Response
                    else if (line.IndexOf("Received Server Response") > 0)
                    {
                        StepTime = line.Substring(line.IndexOf("Client Machine In Time:") + 23, 12);
                        SpanTime = "";
                        Step = "Received Server Response";
                    }
                    //Post-Client Side Validation                
                    else if (line.IndexOf("Post-Client Side Validation") > 0)
                    {
                        SpanTime = line.Substring(line.IndexOf("TimeSpan:") + 9, line.IndexOf(" ms (CID:") - (line.IndexOf("TimeSpan:") + 9));
                        StepTime = "";
                        Step = "Post-Client Side Validation";
                    }
                    //ICD Total Time
                    else if (line.IndexOf("ICD Total Time") > 0)
                    {
                        SpanTime = line.Substring(line.IndexOf("ICD Total Time - ") + 17, line.IndexOf(" ms (CID:") - (line.IndexOf("ICD Total Time - ") + 17));
                        StepTime = "";
                        Step = "ICD Total Time";

                        ICDCode = line.Substring(line.IndexOf("ICD:") + 4, line.IndexOf(", DOS") - (line.IndexOf("ICD:") + 4));
                    }
                    
                    else
                    {
                        continue;
                    }



                    dt_temp.Rows.Add(Date,n, ID, User, CID, Step, ICDCode, StepTime, SpanTime);

                    //DataTable dt = new DataTable();
                    //dt.Columns.Add(new DataColumn("ID"));
                    // dt.Columns.Add(new DataColumn("User"));
                    // dt.Columns.Add(new DataColumn("CID"));
                    // dt.Columns.Add(new DataColumn("Pre-Client Side Validation"));
                    // dt.Columns.Add(new DataColumn("Validate FCodes Total Time"));
                    // dt.Columns.Add(new DataColumn("Send Server Request"));
                    // dt.Columns.Add(new DataColumn("Server Cache"));
                    // dt.Columns.Add(new DataColumn("Received Server Response"));
                    // dt.Columns.Add(new DataColumn("Post-Client Side Validation"));
                    // dt.Columns.Add(new DataColumn("ICD Total Time"));

                    Console.WriteLine(Date + "|" + LineNum +  "|" + ID + "|" + User + "|" + CID + "|" + Step + "|" + ICDCode + "|" + StepTime + "|" + SpanTime);

                }

                DataView dv = dt_temp.DefaultView;
                dv.Sort = "ID desc";
                DataTable sortedDT = dv.ToTable();

                WriteCsv(sortedDT, @"c:\CAOLOG.csv");
                
                
                Console.WriteLine("Press any Key to exit.");
                Console.ReadKey();


            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
        }

        public static void WriteCsv(DataTable dt, string path)
        {
            using (var writer = new StreamWriter(path))
            {
                writer.WriteLine(string.Join(",", dt.Columns.Cast<DataColumn>().Select(dc => dc.ColumnName)));
                foreach (DataRow row in dt.Rows)
                {
                    writer.WriteLine(string.Join(",", row.ItemArray));
                }
            }
        }
    }
}
