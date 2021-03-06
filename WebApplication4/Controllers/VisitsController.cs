﻿using MySql.Data.MySqlClient;
using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using WebApplication4.Models;

namespace WebApplication4.Controllers
{
    public class VisitsController : Controller
    {
        private VisitDbContext db = new VisitDbContext();

        // GET: Visits
        public ActionResult Index(int page = 1)
        {
            List<Visit> listvisits = new List<Visit>();
            MySqlConnection mysql = getMySqlConnection();
            MySqlCommand mySqlCommand = getSqlCommand("select * from user where kind=0 ", mysql);
            mysql.Open();
            MySqlDataReader reader = mySqlCommand.ExecuteReader();

            try
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        Visit visit = new Visit();
                       
                        visit.Name = reader.GetString("name");
                        
                        visit.scannum = reader.GetString("scannum");
                        visit.Identinum = reader.GetString("identinum");


                        
                                            
                        listvisits.Add(visit);

                    }
                }

            }
            catch
            {
                return HttpNotFound();
            }
            finally
            {
                mysql.Close();
            }
            const int pagesize = 8;
            var data = listvisits
                 .OrderBy(p => p.Id).ToPagedList(page, pagesize);
            return View(data);
            //return View(db.DocMangers.ToList());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string Identinum, string scannum, int page = 1)
        {
            List<Visit> listvisits = new List<Visit>();
            MySqlConnection mysql = getMySqlConnection();
            string con;
            if (Identinum == "" && scannum == "")
            {
                con = "select * from user where kind=0";
            }
            else if (Identinum != "" && scannum != "")
            {
                con = "select * from user where kind=0 and (identinum " +
                    "like" + "'%" + Identinum + "%'" + "and scannum like " + "'%" + scannum + "%')";
            }
            else if (Identinum != "" && scannum == "")
            {
                con = "select * from user where kind=0 and identinum " +
                   "like" + "'%" + Identinum + "%'";
            }
            else
            {
                con = "select * from user where kind=0 and " +
                     " scannum like " + "'%" + scannum + "%'";
            }
            MySqlCommand mySqlCommand = getSqlCommand(con, mysql);
            mysql.Open();
            MySqlDataReader reader = mySqlCommand.ExecuteReader();

            try
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        Visit visit = new Visit();

                        visit.Name = reader.GetString("name");
                        
                        visit.scannum = reader.GetString("scannum");
                        visit.Identinum = reader.GetString("identinum");


                       
                        listvisits.Add(visit);

                    }
                }

            }
            catch
            {
                return HttpNotFound();
            }
            finally
            {
                mysql.Close();
            }
            const int pagesize = 8;
            var data = listvisits
                 .OrderBy(p => p.Id).ToPagedList(page, pagesize);
            return View(data);
            //return View(db.DocMangers.ToList());
        }

        public ActionResult CheckAndTreatmentIndex(int? id, string scannum,string identinum)
        {
            Visit visit = new Visit();
      
            visit.scannum = scannum;
            visit.Identinum = identinum;
            DataSet ds = new DataSet();
            ds = GetVisit(scannum);
            visit.getVisit = ds.Tables[0];
            return View(visit);

            //return View(db.DocMangers.ToList());
        }

        // GET: Visits/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Visit visit = new Visit();
            
            MySqlConnection mysql = getMySqlConnection();
            MySqlCommand mySqlCommand = getSqlCommand("select user.name,visits.id," +
                "visits.scannum,visits.visitId,visits.date from user,visits where user.scannum=visits.scannum and visits.id=" + id, mysql);
            mysql.Open();
            MySqlDataReader reader = mySqlCommand.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        visit.Id = reader.GetInt32("id");
                        visit.Name = reader.GetString("name");
                        
                        visit.scannum = reader.GetString("scannum");

                        DateTime b = reader.GetDateTime("date");
                        visit.Date = b.ToString("yyyy-MM-dd");
                        visit.VisitId = reader.GetString("visitId");

                        //visit.detailid = checkreturnvisit(visit.Id);//检测是否有复诊

                        DataSet ds = GetSet(visit.scannum, visit.VisitId);
                        visit.dt = ds.Tables[0];
                        

                        DataSet da = GetTreatment(visit.scannum, visit.VisitId);
                        visit.getTreatment = da.Tables[0];

                        DataSet dd = GetFollowUp(visit.scannum, visit.VisitId);
                        visit.followup = dd.Tables[0];

                        UpdateFollowUp(visit.scannum,visit.VisitId);
                    }
                }


            }
            catch
            {
                return HttpNotFound();
            }
            finally
            {
                mysql.Close();
            }

            return View(visit);
        }
        // GET: Visits/CheckDetails/5
        public ActionResult CheckDetails(int id,string checkname,int refid,int detailid)
        {
            Visit visit = new Visit();
            visit.refid = refid;
            visit.Checkname = checkname;
            visit.detailid = detailid;
            DataSet ds = GetGastroscopy(id);
            visit.weiniaomo = Getweinianmo(id);
            visit.getGastroscopies = ds.Tables[0];
            if (checkname=="胃镜检查")
            {
                string a = Convert.ToString(visit.getGastroscopies.Rows[0][7]);
                DataSet de = GetPathologynum(a);
                visit.pathology = de.Tables[0];
                visit.Checkpathology = GetPathology(a);
            }
          

            return View(visit);
        }
        // GET: Visits/TreatmentDetails/5
        public ActionResult TreatmentDetails(int id)
        {
            Visit visit = new Visit();

            DataSet ds = GetTreatmentDetail(id);
            visit.getTreatmentDetails = ds.Tables[0];
            int treatmentid = Convert.ToInt32(visit.getTreatmentDetails.Rows[0][0]);
           
            visit.getadverseraction = GetAdverseraction(treatmentid);
            return View(visit);
        }


        // GET: Visits/Create
        public ActionResult Create(int?id,string scannum,string identinum)
        {
            Visit visit = new Visit();
            visit.scannum = scannum;
            visit.Identinum = identinum;
            return View(visit);
        }

        // POST: Visits/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create( Visit visit)
        {
            //MySqlCommand mySqlCommand = getSqlCommand("select user.name,user.sex,user.birth,visits.id," +
            //  "visits.scannum,visits.visitId,visits.date from user,visits where user.scannum=visits.scannum ", mysql);
            if (ModelState.IsValid)
            {
                string a = Request.Form["scannum"];
                string b = Request.Form["dateTime"];
                string c = Request.Form["Identinum"];
                //int returnvisit = 2;
                
                //string returnvisitdate = DateTime.Now.ToString("yyyy-MM-dd");
                MySqlConnection mysql = getMySqlConnection();
                MySqlCommand mySqlCommand = getSqlCommand(" INSERT INTO visits (identinum,scannum,visitId,date)VALUES('" + c+
                    "','" + visit.scannum + "','" + visit.VisitId+"',"+"'"+b+"' )", mysql);
                mysql.Open();
                MySqlDataAdapter adapter = new MySqlDataAdapter(mySqlCommand);
                mySqlCommand.ExecuteNonQuery();
                InsertFollowup(visit.VisitId, a);
                mysql.Close();
                return RedirectToAction("Index", "Visits");
            }

            return View(visit);
        }

        // GET: Visits/Create
        public ActionResult returnvisitCreate(int? id, int checkid,int refid)
        {
            Visit visit = new Visit();
            visit.detailid = checkid;//id
            visit.refid = refid;//上一层id
            visit.Checkpathology = checkreturnvisit(checkid);
            if (visit.Checkpathology==1)
            {
                MySqlConnection mysql = getMySqlConnection();
                MySqlCommand mySqlCommand = getSqlCommand("select * from visits where id=" + checkid, mysql);
                mysql.Open();
                MySqlDataReader reader = mySqlCommand.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            visit.dateTime = reader.GetDateTime("returnvisitdate");
                            visit.Date = visit.dateTime.ToString("yyyy-MM-dd");
                            visit.VisitId = reader.GetString("visitid");
                            visit.Name = reader.GetString("returnvisitcontent");
                        }
                    }
                    reader.Close();
                }
                catch
                {
                    return HttpNotFound();
                }
                finally
                {
                    mysql.Close();
                }
            }
            return View(visit);
        }

        // POST: Visits/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult returnvisitCreate( Visit visit)
        {
            //MySqlCommand mySqlCommand = getSqlCommand("select user.name,user.sex,user.birth,visits.id," +
            //  "visits.scannum,visits.visitId,visits.date from user,visits where user.scannum=visits.scannum ", mysql);
            if (ModelState.IsValid)
            {
                string a = Request.Form["detailid"];
                string b = Request.Form["refid"];
                string c = Request.Form["rv"];
                DateTime rvtime =Convert.ToDateTime(Request.Form["rvtime"]);
                string d =rvtime.ToString("yyyy-MM-dd");

                string e = Request.Form["rvcon"];
                MySqlConnection mysql = getMySqlConnection();
                string sql = " UPDATE visits set returnvisit="  + Convert.ToInt32(c) + ",returnvisitdate=" +
               "'" + d + "',returnvisitcontent='" +e+"'"+ " where id=" + Convert.ToInt32(a);
                MySqlCommand mySqlCommand = getSqlCommand(sql, mysql);
                mysql.Open();
                MySqlDataAdapter adapter = new MySqlDataAdapter(mySqlCommand);
                mySqlCommand.ExecuteNonQuery();
                mysql.Close();
                return RedirectToAction("Details/"+b, "Visits");
            }

            return View(visit);
        }


        // GET: Visits/Edit/5
        public ActionResult Edit(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Visit visit = new Visit();
            MySqlConnection mysql = getMySqlConnection();
            MySqlCommand mySqlCommand = getSqlCommand("select * from visits where id=" + id, mysql);
            mysql.Open();
            MySqlDataReader reader = mySqlCommand.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        visit.scannum = reader.GetString("scannum");
                        visit.Id = reader.GetInt32("id");
                        visit.dateTime = reader.GetDateTime("date");
                        visit.Date = visit.dateTime.ToString("yyyy-MM-dd");
                        visit.VisitId = reader.GetString("visitId");

                    }
                }


            }
            catch
            {
                return HttpNotFound();
            }
            finally
            {
                mysql.Close();
            }

            return View(visit);
        }

        // POST: Visits/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,VisitId,dateTime")] Visit visit)
        {
            if (ModelState.IsValid)
            {
                string a = Request.Form["Id"];
                int a1 = Convert.ToInt32(a);
                string b = Request.Form["dateTime"];
                MySqlConnection mysql = getMySqlConnection();
                MySqlCommand mySqlCommand = getSqlCommand(" UPDATE visits set visitId=" +"'"+visit.VisitId+"',date="+
               "'"+b+"'"+"where id=" + a1, mysql);
                mysql.Open();
                MySqlDataAdapter adapter = new MySqlDataAdapter(mySqlCommand);
                mySqlCommand.ExecuteNonQuery();
                mysql.Close();
                return RedirectToAction("Index", "Visits");
            }
            return View(visit);
        }

        // GET: Visits/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Visit visit = new Visit();
            MySqlConnection mysql = getMySqlConnection();
            MySqlCommand mySqlCommand = getSqlCommand("select user.name,visits.id,visits.identinum," +
                "visits.scannum,visits.visitId,visits.date from user,visits where user.scannum=visits.scannum and visits.id=" + id, mysql);
            mysql.Open();
            MySqlDataReader reader = mySqlCommand.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        visit.Id = reader.GetInt32("id");
                        visit.Name = reader.GetString("name");
                        visit.Identinum= reader.GetString("identinum");
                        visit.scannum = reader.GetString("scannum");
                        DateTime b = reader.GetDateTime("date");
                        visit.Date = b.ToString("yyyy-MM-dd");
                        visit.VisitId = reader.GetString("visitId");
                        
                       
                    }
                }


            }
            catch
            {
                return HttpNotFound();
            }
            finally
            {
                mysql.Close();
            }

            return View(visit);
        }

        // POST: Visits/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Visit visit = new Visit();
            MySqlConnection mysql = getMySqlConnection();
            MySqlCommand mySqlCommand = getSqlCommand("SET FOREIGN_KEY_CHECKS=0;DELETE FROM visits where id=" + id, mysql);
            mysql.Open();
            MySqlDataAdapter adapter = new MySqlDataAdapter(mySqlCommand);
            mySqlCommand.ExecuteNonQuery();
            mysql.Close();
            return RedirectToAction("Index");
            
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        private static MySqlConnection getMySqlConnection()
        {
            MySqlConnection mysql = new MySqlConnection(ConfigurationManager.ConnectionStrings["defalutconnection"].ConnectionString);
            return mysql;
        }
        public static MySqlCommand getSqlCommand(String sql, MySqlConnection mysql)
        {
            MySqlCommand mySqlCommand = new MySqlCommand(sql, mysql);

            return mySqlCommand;
        }



        public static DataSet GetGastroscopy(int id)
        {
            MySqlConnection mysql = getMySqlConnection();
            string sql="";
          
                sql = "select id,visit,scannum,checktime,checkname,checknum,conclusion,pathologynum,pathologyconclusion," +
               "other,pic1,pic2,pic3,pic4,pic5,pic6,pic7,pic8,isfollowup,followuptime from gastroscopy where id=" + id;

            MySqlCommand mySqlCommand = getSqlCommand(sql, mysql);
            mysql.Open();
            MySqlDataAdapter command = new MySqlDataAdapter(mySqlCommand);
            mySqlCommand.ExecuteNonQuery();
            mysql.Close();
            DataSet ds = new DataSet();
            command.Fill(ds);
            return ds;
        }

        public static DataSet GetPathologynum(string pathologynum)
        {
            MySqlConnection mysql = getMySqlConnection();
            string sql;
            sql = "select * from pathology where pathologynum='" + pathologynum+"'";
            MySqlCommand mySqlCommand = getSqlCommand(sql, mysql);
            mysql.Open();
            MySqlDataAdapter command = new MySqlDataAdapter(mySqlCommand);
            mySqlCommand.ExecuteNonQuery();
            mysql.Close();
            DataSet ds = new DataSet();
            command.Fill(ds);
            return ds;
        }

        public static DataTable GetAdverseraction(int treatmentid)
        {
            MySqlConnection mysql = getMySqlConnection();
            string sql;
            sql = "select * from adversereactions  where treatmentid=" +  treatmentid  ;
            MySqlCommand mySqlCommand = getSqlCommand(sql, mysql);
            mysql.Open();
            MySqlDataAdapter command = new MySqlDataAdapter(mySqlCommand);
            mySqlCommand.ExecuteNonQuery();
            mysql.Close();
            DataTable ds = new DataTable();
            command.Fill(ds);
            return ds;
        }

        public static DataSet GetSet(string scannum,string VisitID) {
            MySqlConnection mysql = getMySqlConnection();
            
            string sql;
            sql = "select id,checktime,checkname,isfollowup from gastroscopy where scannum=" + "'" + scannum + "'" + "and visit=" + "'" + VisitID+ "' ORDER by checktime desc;";
            MySqlCommand mySqlCommand = getSqlCommand(sql, mysql);
            mysql.Open();
            MySqlDataAdapter command = new MySqlDataAdapter(mySqlCommand);
            mySqlCommand.ExecuteNonQuery();
            mysql.Close();
            DataSet ds = new DataSet();
            command.Fill(ds);
            return ds;
        }

        public static DataSet GetTreatment(string scannum, string VisitID)
        {
            MySqlConnection mysql = getMySqlConnection();
            string sql;
            sql = "select id,time,name,isEradicated,isfollowup from treatment where scannum=" + "'" + scannum + "'" + "and visit=" + "'" + VisitID + "' ORDER by time desc;";
            MySqlCommand mySqlCommand = getSqlCommand(sql, mysql);
            mysql.Open();
            MySqlDataAdapter command = new MySqlDataAdapter(mySqlCommand);
            mySqlCommand.ExecuteNonQuery();
            mysql.Close();
            DataSet ds = new DataSet();
            command.Fill(ds);
            return ds;
        }

        public void InsertFollowup(string visit,string scannum)
        {
            MySqlConnection mysql = getMySqlConnection();
            mysql.Open();
            string dt = DateTime.Now.ToString("yyyy-MM-dd");
            string sql = "INSERT INTO followup (visit,scannum,weijing_check,weijing_check_time," +
                "weinianmo_check,weinianmo_check_time,youmen_check,youmen_check_time,other_check,other_check_time," +
                "youmen_treat,youmen_treat_time,operater_treat,operater_treat_time,other_treat,other_treat_time)" +
                "VALUES('"+visit+"','"+scannum+"',0,'"+dt+"',0,'"+dt+ "',0,'"  + dt + "',0,'" + dt + "',0,'" + 
                dt + "',0,'" + dt + "',0,'" + dt+"')";
            MySqlCommand mySqlCommand = getSqlCommand(sql, mysql);
            MySqlDataAdapter command = new MySqlDataAdapter(mySqlCommand);
            mySqlCommand.ExecuteNonQuery();
            mysql.Close();
        }

        public static int Check_followp(string visit, string scannum )
        {
            int flag = 0;
            MySqlConnection mysql = getMySqlConnection();
            string sql;
            sql = "select * from followup where scannum=" + "'" + scannum + "'" + "and visit=" + "'" + visit + "'";
            MySqlCommand mySqlCommand = getSqlCommand(sql, mysql);
            mysql.Open();
            MySqlDataAdapter command = new MySqlDataAdapter(mySqlCommand);
            mySqlCommand.ExecuteNonQuery();
            mysql.Close();
            DataSet ds = new DataSet();
            command.Fill(ds);
            DataTable dt = ds.Tables[0];
            if (dt.Rows.Count==0)
            {
                flag = 0;
            }
            else
            {
                flag = 1;
            }
            return flag;

        }

        public static DataSet GetTreatmentDetail(int id)
        {
            MySqlConnection mysql = getMySqlConnection();
            string sql;
            sql = "select * from treatment where id=" +id;
            MySqlCommand mySqlCommand = getSqlCommand(sql, mysql);
            mysql.Open();
            MySqlDataAdapter command = new MySqlDataAdapter(mySqlCommand);
            mySqlCommand.ExecuteNonQuery();
            mysql.Close();
            DataSet ds = new DataSet();
            command.Fill(ds);
            return ds;
        }

        public static DataSet GetVisit(string scannum)
        {
            MySqlConnection mysql = getMySqlConnection();
            string sql;
            sql = "select * from visits where scannum='" + scannum+ "' ORDER by date desc";
            MySqlCommand mySqlCommand = getSqlCommand(sql, mysql);
            mysql.Open();
            MySqlDataAdapter command = new MySqlDataAdapter(mySqlCommand);
            mySqlCommand.ExecuteNonQuery();
            mysql.Close();
            DataSet ds = new DataSet();
            command.Fill(ds);
            return ds;
        }

        public string Get_gastroscopy_followuptime(string visit, string scannum, string checkname)
        {
            string sql = "SELECT followuptime FROM `gastroscopy` where visit='" + visit + "' and scannum='" + scannum + "' and checkname='" + checkname + "' " +
                "and isfollowup=1  ORDER BY checktime desc LIMIT 1";
            MySqlConnection mysql = getMySqlConnection();
			mysql.Open();
			MySqlCommand mySqlCommand = getSqlCommand(sql, mysql);
			string gastroscopy_followuptime = "";
			MySqlDataReader reader = mySqlCommand.ExecuteReader();
			try
			{
				while (reader.Read())
				{
					if (reader.HasRows)
					{
						gastroscopy_followuptime = reader.GetString("followuptime");

					}
				}


			}
			catch
			{
				return "";
			}
			finally
			{
				mysql.Close();
			}

			
			return gastroscopy_followuptime;
        }

		public string Get_gastroscopy_followuptime_other(string visit, string scannum)
		{
			string sql = "SELECT followuptime FROM `gastroscopy` where visit='" + visit + "' and scannum='" + scannum + "' and checkname!='胃镜检查' " +
				"and checkname!='幽门螺杆菌检查' and checkname!='胃黏膜血清检查' and isfollowup=1  ORDER BY checktime desc LIMIT 1";
			MySqlConnection mysql = getMySqlConnection();
			mysql.Open();
			MySqlCommand mySqlCommand = getSqlCommand(sql, mysql);
			string gastroscopy_followuptime_other = "";
			MySqlDataReader reader = mySqlCommand.ExecuteReader();
			try
			{
				while (reader.Read())
				{
					if (reader.HasRows)
					{
						gastroscopy_followuptime_other = reader.GetString("followuptime");

					}
				}


			}
			catch
			{
				return "";
			}
			finally
			{
				mysql.Close();
			}


			return gastroscopy_followuptime_other;
		}

		public string Get_treatment_followuptime(string visit, string scannum, int name)
        {
            string sql = "SELECT followuptime FROM `treatment` where visit='" + visit + "' and scannum='" + scannum + "' and `name`="+ name+" and isfollowup=1  ORDER BY time desc LIMIT 1";

            MySqlConnection mysql = getMySqlConnection();
			mysql.Open();
			MySqlCommand mySqlCommand = getSqlCommand(sql, mysql);
			string treatment_followuptime = "";
			MySqlDataReader reader = mySqlCommand.ExecuteReader();
			try
			{
				while (reader.Read())
				{
					if (reader.HasRows)
					{
						treatment_followuptime = reader.GetString("followuptime");

					}
				}


			}
			catch
			{
				return "";
			}
			finally
			{
				mysql.Close();
			}


			return treatment_followuptime;
		}

		public void UpdateFollowUp(string scannum, string VisitID)
		{
			string weijing_check_time = Get_gastroscopy_followuptime(VisitID, scannum, "胃镜检查");
			string weinianmo_check_time = Get_gastroscopy_followuptime(VisitID, scannum, "胃黏膜血清检查");
			string youmen_check_time = Get_gastroscopy_followuptime(VisitID, scannum, "幽门螺杆菌检查");
			string other_check_time = Get_gastroscopy_followuptime_other(VisitID, scannum);
			string youmen_treat_time = Get_treatment_followuptime(VisitID, scannum, 0);
			string other_treat_time = Get_treatment_followuptime(VisitID, scannum, 1);
			string operater_treat_time = Get_treatment_followuptime(VisitID, scannum, 2);

            if (weijing_check_time == ""&& weinianmo_check_time==""&& youmen_check_time==""&&
                other_check_time==""&& youmen_treat_time==""&& other_treat_time==""&& operater_treat_time=="")
            {

            }
            else
            {
                int checkid = Check_followp(VisitID,scannum);
                if (checkid==0)
                {
                    MySqlConnection mysql = getMySqlConnection();
                    mysql.Open();
                    string d = DateTime.Now.ToString("yyyy-MM-dd");
                    string sql = "INSERT INTO followup(visit,scannum,weijing_check,weijing_check_time,weinianmo_check," +
                        "weinianmo_check_time,youmen_check,youmen_check_time,other_check,other_check_time,youmen_treat," +
                        "youmen_treat_time,operater_treat,operater_treat_time,other_treat,other_treat_time)" +
                        " VALUES('" +VisitID + "','" +scannum+"',0,'"+d+"',0,'" + d+"',0,'" + d + "',0,'" + d +
                        "',0,'" + d + "',0,'" + d + "',0,'" + d +"')";
                    MySqlCommand mySqlCommand = getSqlCommand(sql, mysql);
                    MySqlDataAdapter command = new MySqlDataAdapter(mySqlCommand);
                    mySqlCommand.ExecuteNonQuery();
                    mysql.Close();
                }
                if (weijing_check_time != "")
                {
                    MySqlConnection mysql = getMySqlConnection();
                    mysql.Open();
                    string sql = "update followup set weijing_check=1,weijing_check_time='" + weijing_check_time + "' " +
                        "where scannum='" + scannum + "' and visit=" + "'" + VisitID + "' ;";
                    MySqlCommand mySqlCommand = getSqlCommand(sql, mysql);
                    MySqlDataAdapter command = new MySqlDataAdapter(mySqlCommand);
                    mySqlCommand.ExecuteNonQuery();
                    mysql.Close();
                }
                else if (weijing_check_time == "")
                {
                    MySqlConnection mysql = getMySqlConnection();
                    mysql.Open();
                    string sql = "update followup set weijing_check=0,weijing_check_time='" + DateTime.Now + "' " +
                        "where scannum='" + scannum + "' and visit=" + "'" + VisitID + "' ;";
                    MySqlCommand mySqlCommand = getSqlCommand(sql, mysql);
                    MySqlDataAdapter command = new MySqlDataAdapter(mySqlCommand);
                    mySqlCommand.ExecuteNonQuery();
                    mysql.Close();
                }
                if (weinianmo_check_time != "")
                {
                    MySqlConnection mysql = getMySqlConnection();
                    mysql.Open();
                    string sql = "update followup set weinianmo_check=1,weinianmo_check_time='" + weinianmo_check_time + "' " +
                        "where scannum='" + scannum + "' and visit=" + "'" + VisitID + "' ;";
                    MySqlCommand mySqlCommand = getSqlCommand(sql, mysql);
                    MySqlDataAdapter command = new MySqlDataAdapter(mySqlCommand);
                    mySqlCommand.ExecuteNonQuery();
                    mysql.Close();
                }
                else if (weinianmo_check_time == "")
                {
                    MySqlConnection mysql = getMySqlConnection();
                    mysql.Open();
                    string sql = "update followup set weinianmo_check=0,weinianmo_check_time='" + DateTime.Now + "' " +
                        "where scannum='" + scannum + "' and visit=" + "'" + VisitID + "' ;";
                    MySqlCommand mySqlCommand = getSqlCommand(sql, mysql);
                    MySqlDataAdapter command = new MySqlDataAdapter(mySqlCommand);
                    mySqlCommand.ExecuteNonQuery();
                    mysql.Close();
                }
                if (youmen_check_time != "")
                {
                    MySqlConnection mysql = getMySqlConnection();
                    mysql.Open();
                    string sql = "update followup set youmen_check=1,youmen_check_time='" + youmen_check_time + "' " +
                        "where scannum='" + scannum + "' and visit=" + "'" + VisitID + "' ;";
                    MySqlCommand mySqlCommand = getSqlCommand(sql, mysql);
                    MySqlDataAdapter command = new MySqlDataAdapter(mySqlCommand);
                    mySqlCommand.ExecuteNonQuery();
                    mysql.Close();
                }
                else if (youmen_check_time == "")
                {
                    MySqlConnection mysql = getMySqlConnection();
                    mysql.Open();
                    string sql = "update followup set youmen_check=0,youmen_check_time='" + DateTime.Now + "' " +
                        "where scannum='" + scannum + "' and visit=" + "'" + VisitID + "' ;";
                    MySqlCommand mySqlCommand = getSqlCommand(sql, mysql);
                    MySqlDataAdapter command = new MySqlDataAdapter(mySqlCommand);
                    mySqlCommand.ExecuteNonQuery();
                    mysql.Close();
                }
                if (other_check_time != "")
                {
                    MySqlConnection mysql = getMySqlConnection();
                    mysql.Open();
                    string sql = "update followup set other_check=1,other_check_time='" + other_check_time + "' " +
                        "where scannum='" + scannum + "' and visit=" + "'" + VisitID + "' ;";
                    MySqlCommand mySqlCommand = getSqlCommand(sql, mysql);
                    MySqlDataAdapter command = new MySqlDataAdapter(mySqlCommand);
                    mySqlCommand.ExecuteNonQuery();
                    mysql.Close();
                }
                else if (other_check_time == "")
                {
                    MySqlConnection mysql = getMySqlConnection();
                    mysql.Open();
                    string sql = "update followup set other_check=0,other_check_time='" + DateTime.Now + "' " +
                        "where scannum='" + scannum + "' and visit=" + "'" + VisitID + "' ;";
                    MySqlCommand mySqlCommand = getSqlCommand(sql, mysql);
                    MySqlDataAdapter command = new MySqlDataAdapter(mySqlCommand);
                    mySqlCommand.ExecuteNonQuery();
                    mysql.Close();
                }
                if (youmen_treat_time != "")
                {
                    MySqlConnection mysql = getMySqlConnection();
                    mysql.Open();
                    string sql = "update followup set youmen_treat=1,youmen_treat_time='" + youmen_treat_time + "' " +
                        "where scannum='" + scannum + "' and visit=" + "'" + VisitID + "' ;";
                    MySqlCommand mySqlCommand = getSqlCommand(sql, mysql);
                    MySqlDataAdapter command = new MySqlDataAdapter(mySqlCommand);
                    mySqlCommand.ExecuteNonQuery();
                    mysql.Close();
                }
                else if (youmen_treat_time == "")
                {
                    MySqlConnection mysql = getMySqlConnection();
                    mysql.Open();
                    string sql = "update followup set youmen_treat=0,youmen_treat_time='" + DateTime.Now + "' " +
                        "where scannum='" + scannum + "' and visit=" + "'" + VisitID + "' ;";
                    MySqlCommand mySqlCommand = getSqlCommand(sql, mysql);
                    MySqlDataAdapter command = new MySqlDataAdapter(mySqlCommand);
                    mySqlCommand.ExecuteNonQuery();
                    mysql.Close();
                }
                if (other_treat_time != "")
                {
                    MySqlConnection mysql = getMySqlConnection();
                    mysql.Open();
                    string sql = "update followup set other_treat=1,other_treat_time='" + other_treat_time + "' " +
                        "where scannum='" + scannum + "' and visit=" + "'" + VisitID + "' ;";
                    MySqlCommand mySqlCommand = getSqlCommand(sql, mysql);
                    MySqlDataAdapter command = new MySqlDataAdapter(mySqlCommand);
                    mySqlCommand.ExecuteNonQuery();
                    mysql.Close();
                }
                else if (other_treat_time == "")
                {
                    MySqlConnection mysql = getMySqlConnection();
                    mysql.Open();
                    string sql = "update followup set other_treat=0,other_treat_time='" + DateTime.Now + "' " +
                        "where scannum='" + scannum + "' and visit=" + "'" + VisitID + "' ;";
                    MySqlCommand mySqlCommand = getSqlCommand(sql, mysql);
                    MySqlDataAdapter command = new MySqlDataAdapter(mySqlCommand);
                    mySqlCommand.ExecuteNonQuery();
                    mysql.Close();
                }
                if (operater_treat_time != "")
                {
                    MySqlConnection mysql = getMySqlConnection();
                    mysql.Open();
                    string sql = "update followup set operater_treat=1,operater_treat_time='" + operater_treat_time + "' " +
                        "where scannum='" + scannum + "' and visit=" + "'" + VisitID + "' ;";
                    MySqlCommand mySqlCommand = getSqlCommand(sql, mysql);
                    MySqlDataAdapter command = new MySqlDataAdapter(mySqlCommand);
                    mySqlCommand.ExecuteNonQuery();
                    mysql.Close();
                }
                else if (operater_treat_time == "")
                {
                    MySqlConnection mysql = getMySqlConnection();
                    mysql.Open();
                    string sql = "update followup set operater_treat=0,operater_treat_time='" + DateTime.Now + "' " +
                        "where scannum='" + scannum + "' and visit=" + "'" + VisitID + "' ;";
                    MySqlCommand mySqlCommand = getSqlCommand(sql, mysql);
                    MySqlDataAdapter command = new MySqlDataAdapter(mySqlCommand);
                    mySqlCommand.ExecuteNonQuery();
                    mysql.Close();
                }
            }

			
		}

        public DataSet GetFollowUp(string scannum, string VisitID)
        {
			UpdateFollowUp(scannum,VisitID);
			MySqlConnection mysql = getMySqlConnection();
            string sql;
            sql = "select * from followup where scannum=" + "'" + scannum + "'" + "and visit=" + "'" + VisitID + "' ;";
            MySqlCommand mySqlCommand = getSqlCommand(sql, mysql);
            mysql.Open();
            MySqlDataAdapter command = new MySqlDataAdapter(mySqlCommand);
            mySqlCommand.ExecuteNonQuery();
            mysql.Close();
            DataSet ds = new DataSet();
            command.Fill(ds);
            return ds;
        }

        public static string getstring(string str, int length)
        {
            int i = 0, j = 0;
            foreach (char chr in str)
            {
                if ((int)chr > 127)
                {
                    i += 2;
                }
                else
                {
                    i++;
                }
                if (i > length)
                {
                    str = str.Substring(0, j) + "...";
                    break;
                }
                j++;
            }
            return str;

        }

        public ActionResult PreViewing()
        {

            return View();
        }

        private List<SelectListItem> GetSexList()
        {
            return new List<SelectListItem>() {
              new SelectListItem
              {
                     Text = "男",
                     Value = "1"
              },new SelectListItem
              {
                     Text = "女",
                     Value = "0"
              }
       };
        }
        private int GetPathology(string pathologynum)
        {
            int rawId = 0;
            MySqlConnection mysql = getMySqlConnection();
            MySqlCommand mySqlCommand = getSqlCommand("select id from pathology where pathologynum=" + "'" + pathologynum + "'", mysql);
            mysql.Open();
            MySqlDataReader reader = mySqlCommand.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        rawId = reader.GetInt32("id");

                    }
                }
                reader.Close();
            }
            catch
            {

            }
            return rawId;
        }

        private int checkreturnvisit(int id)
        {
            int rawId = 0;
            MySqlConnection mysql = getMySqlConnection();
            MySqlCommand mySqlCommand = getSqlCommand("select returnvisit from visits where id=" + id, mysql);
            mysql.Open();
            MySqlDataReader reader = mySqlCommand.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        rawId = reader.GetInt32("returnvisit");

                    }
                }
                reader.Close();
            }
            catch
            {

            }
            return rawId;
        }

        private DataTable Getweinianmo(int gasid)
        {
            MySqlConnection mysql = getMySqlConnection();
            string sql;
            sql = "select * from gasweinianmo where gasid=" + gasid;
            MySqlCommand mySqlCommand = getSqlCommand(sql, mysql);
            mysql.Open();
            MySqlDataAdapter command = new MySqlDataAdapter(mySqlCommand);
            mySqlCommand.ExecuteNonQuery();
            mysql.Close();
            DataTable dt = new DataTable();
            command.Fill(dt);
            return dt;
        }
    }
}
