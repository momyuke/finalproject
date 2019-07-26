﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FinalProject.Controllers;
using FinalProject.DTO;
using FinalProject.Filters;
using FinalProject.Utils;
using FinalProject.Models;

namespace FinalProject.Controllers
{
    [UserAccessCandidateFilter]
    public class CandidateController : Controller
    {
        [Route("candidate")]
        public ActionResult Index()
        {
            return Redirect("~/candidate/preselection");
        }
        //################################################ Sub Menu Candidate Preselection ################################# 

        //********************************************************** Manage Data Candidate **********************************************************

        //------------------------------------------------------- for view candidate preselection -----------------------------------------------
        [Route("candidate/preselection")]
        public ActionResult CandidatePreselection()
        {
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    //---------------------------- prepare data candidate for show in view --------------
                    //note : data candidate from class Manage_CandidateSelectionHistoryDTO method GetDataSelectionHistory
                    //not  : data in this view especialy for candidate where state_id is 1,10 or 11 (state in step preselection)
                    List<CandidateSelectionHistoryDTO> ListCandidate = Manage_CandidateSelectionHistoryDTO.GetDataSelectionHistory().Where(d =>
                    d.CANDIDATE_STATE == 1 || d.CANDIDATE_STATE == 10 || d.CANDIDATE_STATE == 11).ToList();

                    //---------------------------- prepare data viewbag --------------------
                    ViewBag.DataView = new Dictionary<string, object>{
                    {"title","Preselection"},
                    {"ListPosition",Manage_JobPositionDTO.GetData()},
                    {"ListState",Manage_StateCandidateDTO.GetData().Where(d => d.ID == 1 || d.ID == 10 || d.ID == 11)}
                    };

                    //============================ process searchng ============================
                    if (Request["filter"] != null)
                    {
                        string Position = Request["POSITION"];
                        int StateId = Convert.ToInt16(Request["CANDIDATE_STATE"]);
                        string Keyword = Request["Keyword"];

                        if (StateId != 0 && (Position == "all" && Keyword == ""))
                        {
                            ListCandidate = ListCandidate.Where(d => d.CANDIDATE_STATE == StateId).ToList();
                        }
                        if (Position != "all" && (StateId == 0 && Keyword == ""))
                        {
                            ListCandidate = ListCandidate.Where(d =>
                            d.CANDIDATE_APPLIED_POSITION == Position ||
                            d.CANDIDATE_SUITABLE_POSITION == Position &&
                            (d.CANDIDATE_STATE == 1 || d.CANDIDATE_STATE == 10 || d.CANDIDATE_STATE == 11)).ToList();
                        }
                        if (Keyword != "" && (StateId == 0 && Position == "all"))
                        {
                            ListCandidate = ListCandidate.Where(d =>
                                d.CANDIDATE_EMAIL.Contains(Keyword) ||
                                d.CANDIDATE_NAME.Contains(Keyword) ||
                                d.CANDIDATE_PHONE.Contains(Keyword) &&
                                (d.CANDIDATE_STATE == 1 || d.CANDIDATE_STATE == 10 || d.CANDIDATE_STATE == 11)).ToList();
                        }
                        else
                        {
                            ListCandidate = ListCandidate.Where(d =>
                             d.CANDIDATE_APPLIED_POSITION == Position ||
                             d.CANDIDATE_SUITABLE_POSITION == Position ||
                             d.CANDIDATE_STATE == StateId ||
                             d.CANDIDATE_EMAIL.Contains(Keyword) ||
                             d.CANDIDATE_NAME.Contains(Keyword) ||
                             d.CANDIDATE_PHONE.Contains(Keyword) &&
                             (d.CANDIDATE_STATE == 1 || d.CANDIDATE_STATE == 10 || d.CANDIDATE_STATE == 11)).ToList();
                        }
                    }
                    //============================ end process searchng ============================

                    //return view
                    return View("Preselection/Index", ListCandidate);
                }
            }
            catch (Exception)
            {
                return Redirect("~/auth/error");
            }
        }

        //----------------------------------------------------------- view form add new candidate -----------------------------------------
        [Route("candidate/preselection/create/candidate")]
        public ActionResult CandidatePreselectionAdd()
        {
            try
            {
                using (DBEntities db = new DBEntities())
                {

                    ViewBag.DataView = new Dictionary<string, object>(){
                    {"title","Preselection"}
                    };
                    return View("Preselection/AddCandidate");
                }
            }
            catch (Exception)
            {
                return Redirect("~/auth/error");
            }
        }

        //---------------------------------------------------------- View Detail candidate ------------------------------------------------
        [Route("candidate/preselection/read/detailcandidate/{id?}")]
        public ActionResult DetailCandidate(string id = null)
        {
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    if (id == null) return Redirect("~/candidate/preselection");

                    int candidateId = Convert.ToInt16(id);

                    DetailCandidateDTO DataDetail = Manage_DetailCandidate.GetData(candidateId);

                    if (DataDetail == null) return Redirect("~/candidate/preselection");

                    ViewBag.DataView = new Dictionary<string, object>()
                    {
                        {"title","Preselection"}
                    };

                    return View("Preselection/DetailCandidate", DataDetail);
                }

            }
            catch (Exception)
            {
                return Redirect("~/auth/error");
            }
        }

        //-------------------------------------------------- PROCESS ADD NEW CANDIDATE --------------------------------------
        [Route("candidate/preselection/create/candidate/process")]
        public ActionResult CandidatePreselectionAdd(CandidateDTO DataNewCandidate,  HttpPostedFileBase Pict = null, HttpPostedFileBase Cv = null)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //process add will return list object, [0] is return from db.saveCahnge() and [1] return candidate_id (CA******)
                    var ProcessAdd = Manage_CandidateDTO.AddData(DataNewCandidate,Pict,Cv);

                    if (Convert.ToInt16(ProcessAdd[0]) > 0)
                    {
                        TempData.Add("message", "New Candidate added successfully");
                        TempData.Add("type", "success");

                        UserLogingUtils.SaveLoggingUserActivity("add new Candidate"+ Convert.ToString(ProcessAdd[1]));
                    }
                    else
                    {
                        TempData.Add("message", "New Candidate failed to add");
                        TempData.Add("type", "warning");
                    }
                    return Redirect("~/candidate/preselection");
                }

                TempData.Add("message", "New Candidate failed to add, please complete form add");
                TempData.Add("type", "danger");
            
                return Redirect("~/candidate/preselection");
            }
            catch (Exception)
            {
                return Redirect("~/auth/error");
            }
        }

        //------------------------------------------ VIEW EDIT CANDIDATE ---------------------------------------------------
        [Route("candidate/preselection/update/candidate/{id?}")]
        public ActionResult CandidateEdit(string id = null)
        {
            try
            {
                if (id == null)return Redirect("~/candidate/preselection");

                int CandidateId = Convert.ToInt16(id);
                CandidateDTO DataCandidate = Manage_CandidateDTO.GetDataCandidate().FirstOrDefault(d => d.ID == CandidateId);

                if (DataCandidate == null) return Redirect("~/candidate/preselection");

                ViewBag.DataView = new Dictionary<string, object>()
                {
                    {"title","preselection"},
                    {"ListState",Manage_StateCandidateDTO.GetData().Where(d => d.ID == 2 || d.ID == 10 || d.ID == 1 || d.ID == 11).ToList() }
                };

                return View("Preselection/EditCandidate", DataCandidate);
            }
            catch
            {
                return Redirect("~/auth/error");
            }
        }



        //------------------------------------------ Process Edit Data Candidate -------------------------------------------------
        [Route("candidate/preselection/update/candidate/process")]
        public ActionResult CandidateEdit(CandidateDTO Data, HttpPostedFileBase Pict = null, HttpPostedFileBase Cv = null)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var ProcessEdit = Manage_CandidateDTO.EditCandidate(Data, Pict, Cv);

                    if (ProcessEdit > 0)
                    {
                        TempData.Add("message", "Candidate Update successfully");
                        TempData.Add("type", "success");
                        UserLogingUtils.SaveLoggingUserActivity("Edit Candidate" + Manage_CandidateDTO.GetDataCandidate().FirstOrDefault(d => d.ID == Data.ID));
                    }
                    else
                    {
                        TempData.Add("message", "Candidate failed to Update");
                        TempData.Add("type", "warning");
                    }

                    return Redirect("~/candidate/preselection");
                }
                TempData.Add("message", "Candidate failed to Update, please complete form edit");
                TempData.Add("type", "danger");
                return Redirect("~/candidate/preselection");
            }
            catch
            {
                return Redirect("~/auth/error");
            }
        }

        //************************************************* JOB EXPERIENCE OF CANDIDATE *****************************************************


        //----------------------------------------------------- process add new job experience ------------------------------
        [Route("candidate/preselection/create/jobExp")]
        public ActionResult JobExpAdd(CandidateJobExperienceDTO NewJobExp)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (DBEntities db = new DBEntities())
                    {
                        var ProcessAdd = Manage_CandidateJobExperienceDTO.AddData(NewJobExp);

                        if (ProcessAdd > 0)
                        {
                            if (TempData.Peek("message") != null)
                            {
                                TempData.Remove("message");
                                TempData.Remove("type");
                            }
                            TempData.Add("message", "Candidate new job experience added successfully");
                            TempData.Add("type", "success");
                            UserLogingUtils.SaveLoggingUserActivity("add job experience Candidate " + NewJobExp.CANDIDATE_ID + " Job Experience in " + NewJobExp.COMPANY_NAME);
                        }
                        else
                        {
                            if (TempData.Peek("message") != null)
                            {
                                TempData.Remove("message");
                                TempData.Remove("type");
                            }
                            TempData.Add("message", "Candidate new job experience failed to add");
                            TempData.Add("type", "warning");
                        }
                        return Redirect("~/candidate/preselection/read/detailcandidate/" + NewJobExp.CANDIDATE_ID);
                    }
                }

                TempData.Add("message", "Candidate new job experience failed to add please complete form add");
                TempData.Add("type", "danger");
                return Redirect("~/candidate/preselection/read/detailcandidate/" + NewJobExp.CANDIDATE_ID);
        }
            catch (Exception)
            {
                return Redirect("~/auth/error");
    }
}

        //----------------------------------------------------------- view edit job exp ------------------------------------
        [Route("candidate/preselection/update/jobExp/{id?}")]
        public ActionResult JobExp(string id = null)
        {
            try
            {
                ViewBag.DataView = new Dictionary<string, object>()
                {
                    {"title","Preselection"}
                };
                int JobExpId = Convert.ToInt16(id);
                CandidateJobExperienceDTO Data = Manage_CandidateJobExperienceDTO.GetData().FirstOrDefault(d => d.ID == JobExpId);
                return View("Preselection/EditJobExp",Data);
            }
            catch(Exception)
            {
                return Redirect("~/auth/error");
            }
        }

        //----------------------------------------------------------- process edit job exp ---------------------------------
        [Route("candidate/preselection/update/jobExp/process")]
        public ActionResult JobExpEdit(CandidateJobExperienceDTO NewJobExp)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (DBEntities db = new DBEntities())
                    {
                        var ProcessEdit = Manage_CandidateJobExperienceDTO.EditData(NewJobExp);

                        if (ProcessEdit > 0)
                        {
                            if(TempData.Peek("message") != null)
                            {
                                TempData.Remove("message");
                                TempData.Remove("type");
                            }
                            TempData.Add("message", "Candidate job experience edited successfully");
                            TempData.Add("type", "success");
                            UserLogingUtils.SaveLoggingUserActivity("edit job experience Candidate " + NewJobExp.CANDIDATE_ID + " Job Experience in " + NewJobExp.COMPANY_NAME);
                        }
                        else
                        {
                            if (TempData.Peek("message") != null)
                            {
                                TempData.Remove("message");
                                TempData.Remove("type");
                            }
                            TempData.Add("message", "Candidate job experience failed to edit");
                            TempData.Add("type", "warning");
                        }
                        return Redirect("~/candidate/preselection/edit/jobExp/"+NewJobExp.ID);
                    }
                }

                TempData.Add("message", "Candidate job experience failed to edit please complete form edit");
                TempData.Add("type", "danger");
                return Redirect("~/candidate/preselection/edit/jobExp/" + NewJobExp.ID);

            }
            catch (Exception)
            {
                return Redirect("~/auth/error");
            }
        }











        //################################################# CANDIDATE CALL ###############################################################

        //------------------------------------------------- View for candidate call -----------------------------------------------------
        [Route("candidate/call")]
        public ActionResult CandidateCall()
        {
            try
            {
                //---------------------------- prepare data candidate for show in view --------------
                //note : data candidate from class Manage_CandidateSelectionHistoryDTO method GetDataSelectionHistory
                //note : data in this view especialy for candidate where state_id is 2(call) or 18(called) (state in step call)
                List<CandidateSelectionHistoryDTO> ListCandidate = Manage_CandidateSelectionHistoryDTO.GetDataSelectionHistory().Where(d =>
                d.CANDIDATE_STATE == 2 || d.CANDIDATE_STATE == 18).ToList();
                //prepare vew bag
                //---------------------------- prepare data viewbag --------------------
                ViewBag.DataView = new Dictionary<string, object>{
                    {"title","Call"},
                    {"ListPosition",Manage_JobPositionDTO.GetData()},
                    {"ListState",Manage_StateCandidateDTO.GetData().Where(d => d.ID == 2 || d.ID == 18)}
                    };

                //============================ process searchng ============================
                if (Request["filter"] != null)
                {
                    string Position = Request["POSITION"];
                    int StateId = Convert.ToInt16(Request["CANDIDATE_STATE"]);
                    string Keyword = Request["Keyword"];

                    if (StateId != 0 && (Position == "all" && Keyword == ""))
                    {
                        ListCandidate = ListCandidate.Where(d => d.CANDIDATE_STATE == StateId).ToList();
                    }
                    if (Position != "all" && (StateId == 0 && Keyword == ""))
                    {
                        ListCandidate = ListCandidate.Where(d =>
                        d.CANDIDATE_APPLIED_POSITION == Position ||
                        d.CANDIDATE_SUITABLE_POSITION == Position &&
                        (d.CANDIDATE_STATE == 2 || d.CANDIDATE_STATE == 18)).ToList();
                    }
                    if (Keyword != "" && (StateId == 0 && Position == "all"))
                    {
                        ListCandidate = ListCandidate.Where(d =>
                        d.CANDIDATE_EMAIL.Contains(Keyword) ||
                        d.CANDIDATE_NAME.Contains(Keyword) ||
                        d.CANDIDATE_PHONE.Contains(Keyword) &&
                            (d.CANDIDATE_STATE == 2 || d.CANDIDATE_STATE == 18)).ToList();
                    }
                    else
                    {
                        ListCandidate = ListCandidate.Where(d =>
                         d.CANDIDATE_APPLIED_POSITION == Position ||
                         d.CANDIDATE_SUITABLE_POSITION == Position ||
                         d.CANDIDATE_STATE == StateId ||
                         d.CANDIDATE_EMAIL.Contains(Keyword) ||
                         d.CANDIDATE_NAME.Contains(Keyword) ||
                         d.CANDIDATE_PHONE.Contains(Keyword) &&
                         (d.CANDIDATE_STATE == 2 || d.CANDIDATE_STATE == 18)).ToList();
                    }
                }
                //============================ end process searchng ============================



                return View("Call/Call", ListCandidate);
            }
            catch (Exception)
            {
                return Redirect("~/auth/error");
            }
        }


        //------------------------------------------------- View for candidate !!! CALLED !!! ---------------------------------------------
        [Route("candidate/call/read/called")]
        public ActionResult CandidateCalled()
        {
            try
            {
                //---------------------------- prepare data candidate for show in view --------------
                //note : data candidate from class Manage_CandidateSelectionHistoryDTO method GetDataSelectionHistory
                //note : data in this view especialy for candidate where state_id is 2(call) or 18(called) (state in step call)
                List<CandidateSelectionHistoryDTO> ListCandidate = Manage_CandidateSelectionHistoryDTO.GetDataSelectionHistory().Where(d =>
            d.CANDIDATE_STATE == 8 || d.CANDIDATE_STATE == 15 || d.CANDIDATE_STATE == 16 || d.CANDIDATE_STATE == 17).ToList();
            //prepare vew bag
            //---------------------------- prepare data viewbag --------------------
            ViewBag.DataView = new Dictionary<string, object>{
                    {"title","Call"},
                    {"ListPosition",Manage_JobPositionDTO.GetData()},
                    {"ListState",Manage_StateCandidateDTO.GetData().Where(d => d.ID == 8)}
                    };

            //============================ process searchng ============================
            if (Request["filter"] != null)
            {
                string Position = Request["POSITION"];
                int StateId = Convert.ToInt16(Request["CANDIDATE_STATE"]);
                string Keyword = Request["Keyword"];

                if (StateId != 0 && (Position == "all" && Keyword == ""))
                {
                    ListCandidate = ListCandidate.Where(d => d.CANDIDATE_STATE == StateId).ToList();
                }
                if (Position != "all" && (StateId == 0 && Keyword == ""))
                {
                    ListCandidate = ListCandidate.Where(d =>
                    d.CANDIDATE_APPLIED_POSITION == Position ||
                    d.CANDIDATE_SUITABLE_POSITION == Position &&
                    (d.CANDIDATE_STATE == 2 || d.CANDIDATE_STATE == 8)).ToList();
                }
                if (Keyword != "" && (StateId == 0 && Position == "all"))
                {
                    ListCandidate = ListCandidate.Where(d =>
                    d.CANDIDATE_EMAIL.Contains(Keyword) ||
                    d.CANDIDATE_NAME.Contains(Keyword) ||
                    d.CANDIDATE_PHONE.Contains(Keyword) &&
                        (d.CANDIDATE_STATE == 2 || d.CANDIDATE_STATE == 8)).ToList();
                }
                else
                {
                    ListCandidate = ListCandidate.Where(d =>
                     d.CANDIDATE_APPLIED_POSITION == Position ||
                     d.CANDIDATE_SUITABLE_POSITION == Position ||
                     d.CANDIDATE_STATE == StateId ||
                     d.CANDIDATE_EMAIL.Contains(Keyword) ||
                     d.CANDIDATE_NAME.Contains(Keyword) ||
                     d.CANDIDATE_PHONE.Contains(Keyword) &&
                     (d.CANDIDATE_STATE == 2 || d.CANDIDATE_STATE == 18)).ToList();
                }
            }
            //============================ end process searchng ============================

            return View("Call/Called", ListCandidate);

            }
            catch (Exception)
            {
                return Redirect("~/auth/error");
            }
        }

        //---------------------------------------------------------- View next call to called ----------------------------------------
        [Route("candidate/call/update/next/{id?}")]
        public ActionResult CallNext(string id = null)
        {
            try
            {
                if (id == null) return Redirect("~/candidate/call");

                int CandidateId = Convert.ToInt16(id);
                CandidateDTO DataCandidate = Manage_CandidateDTO.GetDataCandidate().FirstOrDefault(d => d.ID == CandidateId);

                if (DataCandidate == null) return Redirect("~/candidate/preselection");

                ViewBag.DataView = new Dictionary<string, object>()
                {
                    {"title","Call"},
                    {"ListState",Manage_StateCandidateDTO.GetData().Where(d => d.ID == 8 ||  d.ID == 2).ToList() }
                };

                return View("Call/EditCandidateCall", DataCandidate);
            }
            catch
            {
                return Redirect("~/auth/error");
            }
        }

        //------------------------------------------ Process call next to called -------------------------------------------------
        [Route("candidate/call/update/next/process")]
        public ActionResult CandidateCallNext(CandidateDTO Data, HttpPostedFileBase Pict = null, HttpPostedFileBase Cv = null)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var ProcessEdit = Manage_CandidateDTO.EditCandidate(Data, Pict, Cv);

                    //process removing state call

                    if (ProcessEdit > 0)
                    {
                        TempData.Add("message", "Candidate Update successfully");
                        TempData.Add("type", "success");
                        UserLogingUtils.SaveLoggingUserActivity("Edit Candidate" + Manage_CandidateDTO.GetDataCandidate().FirstOrDefault(d => d.ID == Data.ID));
                        using (DBEntities db = new DBEntities())
                        {
                            var SelectHis = db.TB_CANDIDATE_SELECTION_HISTORY.FirstOrDefault(d => d.CANDIDATE_STATE == 2 && d.CANDIDATE_ID == Data.ID);
                            db.TB_CANDIDATE_SELECTION_HISTORY.Remove(SelectHis);
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        TempData.Add("message", "Candidate failed to Update");
                        TempData.Add("type", "warning");
                    }

                    return Redirect("~/candidate/call/read/called");
                }
                TempData.Add("message", "Candidate failed to Update, please complete form edit");
                TempData.Add("type", "danger");
                return Redirect("~/candidate/call");
            }
            catch
            {
                return Redirect("~/auth/error");
            }
        }

        //---------------------------------------------------------- View next called to Interview ----------------------------------------
        [Route("candidate/call/update/called/next/{id?}")]
        public ActionResult CalledNext(string id = null)
        {
            try
            {
                if (id == null) return Redirect("~/candidate/call");

                int CandidateId = Convert.ToInt16(id);
                CandidateDTO DataCandidate = Manage_CandidateDTO.GetDataCandidate().FirstOrDefault(d => d.ID == CandidateId);

                if (DataCandidate == null) return Redirect("~/candidate/preselection");

                ViewBag.DataView = new Dictionary<string, object>()
                {
                    {"title","Call"},
                    {"ListState",Manage_StateCandidateDTO.GetData().Where(d => d.ID == 8 ||  d.ID == 19).ToList() }
                };

                return View("Call/EditCandidateCall", DataCandidate);
            }
            catch
            {
                return Redirect("~/auth/error");
            }
        }

        //------------------------------------------ Process called next to interview -------------------------------------------------
        [Route("candidate/call/update/called/next/process")]
        public ActionResult CandidateCalledNext(CandidateDTO Data, HttpPostedFileBase Pict = null, HttpPostedFileBase Cv = null)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var ProcessEdit = Manage_CandidateDTO.EditCandidate(Data, Pict, Cv);

                    if (ProcessEdit > 0)
                    {
                        TempData.Add("message", "Candidate Update successfully");
                        TempData.Add("type", "success");
                        UserLogingUtils.SaveLoggingUserActivity("Edit Candidate" + Manage_CandidateDTO.GetDataCandidate().FirstOrDefault(d => d.ID == Data.ID));
                    }
                    else
                    {
                        TempData.Add("message", "Candidate failed to Update");
                        TempData.Add("type", "warning");
                    }

                    return Redirect("~/candidate/call/read/called");
                }
                TempData.Add("message", "Candidate failed to Update, please complete form edit");
                TempData.Add("type", "danger");
                return Redirect("~/candidate/call");
            }
            catch
            {
                return Redirect("~/auth/error");
            }
        }








        //------------------------------------------------------------ candidate interview -----------------------------------------

        [Route("candidate/interview")]
        public ActionResult CandidateInterview()
        {
            try
            {
                //---------------------------- prepare data candidate for show in view --------------
                //note : data candidate from class Manage_CandidateSelectionHistoryDTO method GetDataSelectionHistory
                //note : data in this view especialy for candidate where state_id is 19(interview process)
                List<CandidateSelectionHistoryDTO> ListCandidate = Manage_CandidateSelectionHistoryDTO.GetDataSelectionHistory().Where(d =>
            d.CANDIDATE_STATE == 19).ToList();
                //prepare vew bag
                //---------------------------- prepare data viewbag --------------------
                ViewBag.DataView = new Dictionary<string, object>{
                    {"title","Call"},
                    {"ListPosition",Manage_JobPositionDTO.GetData()},
                    {"ListState",Manage_StateCandidateDTO.GetData().Where(d => d.ID == 15 || d.ID == 16 || d.ID == 17)}
                    };

                //============================ process searchng ============================
                if (Request["filter"] != null)
                {
                    string Position = Request["POSITION"];
                    int StateId = Convert.ToInt16(Request["CANDIDATE_STATE"]);
                    string Keyword = Request["Keyword"];

                    if (StateId != 0 && (Position == "all" && Keyword == ""))
                    {
                        ListCandidate = ListCandidate.Where(d => d.CANDIDATE_STATE == StateId).ToList();
                    }
                    if (Position != "all" && (StateId == 0 && Keyword == ""))
                    {
                        ListCandidate = ListCandidate.Where(d =>
                        d.CANDIDATE_APPLIED_POSITION == Position ||
                        d.CANDIDATE_SUITABLE_POSITION == Position &&
                        (d.CANDIDATE_STATE == 2 || d.CANDIDATE_STATE == 8)).ToList();
                    }
                    if (Keyword != "" && (StateId == 0 && Position == "all"))
                    {
                        ListCandidate = ListCandidate.Where(d =>
                        d.CANDIDATE_EMAIL.Contains(Keyword) ||
                        d.CANDIDATE_NAME.Contains(Keyword) ||
                        d.CANDIDATE_PHONE.Contains(Keyword) &&
                            (d.CANDIDATE_STATE == 2 || d.CANDIDATE_STATE == 8)).ToList();
                    }
                    else
                    {
                        ListCandidate = ListCandidate.Where(d =>
                         d.CANDIDATE_APPLIED_POSITION == Position ||
                         d.CANDIDATE_SUITABLE_POSITION == Position ||
                         d.CANDIDATE_STATE == StateId ||
                         d.CANDIDATE_EMAIL.Contains(Keyword) ||
                         d.CANDIDATE_NAME.Contains(Keyword) ||
                         d.CANDIDATE_PHONE.Contains(Keyword) &&
                         (d.CANDIDATE_STATE == 2 || d.CANDIDATE_STATE == 18)).ToList();
                    }
                }
                //============================ end process searchng ============================

                return View("Call/Called", ListCandidate);

            }
            catch (Exception)
            {
                return Redirect("~/auth/error");
            }
        }


        //------------------------------------------------------------ candidate delivery -----------------------------------------

        [Route("candidate/delivery")]
        public ActionResult CandidateDelivery()
        {
            try
            {
                ViewBag.DataView = new Dictionary<string, object>()
                {
                    {"title","Candidate - Delivery"}
                };
                return View("Delivery/Index");
            }
            catch (Exception)
            {
                return Redirect("~/auth/error");
            }
        }


    }
}