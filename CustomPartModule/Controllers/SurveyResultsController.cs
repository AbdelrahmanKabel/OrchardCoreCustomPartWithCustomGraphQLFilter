using CustomPartModule.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using YesSql;
using ISession = YesSql.ISession;

namespace CustomPartModule.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class SurveyResultsController : Controller
    {
        private readonly IOrchardHelper _orchardHelper;
        private readonly IContentManager _contentManager;
        private readonly ISession _session;


        public SurveyResultsController(IContentManager contentManager, ISession session, IOrchardHelper orchardHelper)
        {
            _contentManager = contentManager;
            _session = session;
            _orchardHelper = orchardHelper;
        }
        public IActionResult Index(SurveyModel survey)
        {
           var surveysList = GetSurveysList().Select(x => new SelectListItem{ Value = x.SurveyId, Text = x.SurveyTitle }).ToList();
           var usersAnswerdList = GetUsersList().Select(x => new SelectListItem{ Value = x.UserId, Text = x.UserName }).ToList();

            ViewBag.SurveysList = surveysList;
            ViewBag.UsersAnswerdList = usersAnswerdList;
            SurveyModel model = null;
            if (!string.IsNullOrEmpty(survey.SurveyId) && !string.IsNullOrEmpty(survey.UserId))
            {
                model = GetUserSurveyAnswerAsync(survey.SurveyId, survey.UserId).Result;
                ViewBag.UserName = usersAnswerdList.FirstOrDefault(u => u.Value == survey.UserId)?.Text;
            }
            else if(!string.IsNullOrEmpty(survey.SurveyId))
            {
                model = GetSurveyAnswersAsync(survey.SurveyId).Result;
            }
            return View(model);
        }

        public async Task<SurveyModel> GetSurveyAnswersAsync(string surveyId)
        {
            try
            {
                var survey = await _orchardHelper.GetContentItemByIdAsync(surveyId);
                if(survey == null)
                {
                    return null;
                }
                var surveyObject = survey.As<Survey>();
                var answerContentItems = await _session.Query<ContentItem, ContentItemIndex>()
                       .Where(x => x.ContentType == "SurveyAnswer" && x.Latest == true && x.Published == true)
                       .ListAsync();
                var surveyAnswers = answerContentItems.Select(x => x.As<SurveyAnswer>()).Where(a => a.SurveyId.Text == surveyId).ToList();

                List<QuestionModel> questions = new List<QuestionModel>();

                SurveyModel model = new SurveyModel();
                model.SurveyId = surveyId;
                model.SurveyTitle = survey.DisplayText;

                string[] questionTypes = { "MultipleChoiceQuestion", "BooleanQuestion" };
                if (surveyAnswers.Any() && surveyObject.ShowAnswers.Value)
                {
                    foreach (var question in survey.Content.BagPart.ContentItems) //Survey questions
                    {
                        //if (questionTypes.Contains(item.QuestionType.Text))
                        if (question.ContentType == "MultipleChoiceQuestion") // Multiple Choice Question
                        {
                            List<AnswerModel> answers = new List<AnswerModel>();

                            QuestionModel questionModel = new QuestionModel();
                            questionModel.QuestionId = question.ContentItemId;
                            questionModel.QuestionType = question.ContentType;
                            questionModel.AllowMultipleAnswers = question.MultipleChoiceQuestion.AllowMultipleAnswers.Value;
                            questionModel.QuestionTitle = question.DisplayText;
                            questionModel.QuestionCount = questionModel.AllowMultipleAnswers ? surveyAnswers.Where(a => a.QuestionId.Text == questionModel.QuestionId).Select(s => s.UserId.Text).Distinct().Count() : surveyAnswers.Count(a => a.QuestionId.Text == questionModel.QuestionId); //Wrong value when multiple answers
                            int questionCount = surveyAnswers.Count(a => a.QuestionId.Text == questionModel.QuestionId);
                            foreach (var item in question.BagPart.ContentItems)
                            {
                                AnswerModel answerModel = new AnswerModel();
                                answerModel.AnswerId = item.ContentItemId;
                                answerModel.AnswerTitle = item.DisplayText;
                                answerModel.AnswerCount = surveyAnswers.Count(a => a.QuestionId.Text == questionModel.QuestionId && a.AnswerId.Text == answerModel.AnswerId);
                                answerModel.AnswerPercentage = (answerModel.AnswerCount * 100) / questionCount;
                                answers.Add(answerModel);
                            }
                            questionModel.Answers = answers;
                            questions.Add(questionModel);
                        }
                        else if (question.ContentType == "BooleanQuestion")
                        {
                            QuestionModel questionModel = new QuestionModel();
                            questionModel.QuestionId = question.ContentItemId;
                            questionModel.QuestionType = question.ContentType;
                            questionModel.QuestionTitle = question.DisplayText;
                            questionModel.QuestionCount = surveyAnswers.Count(a => a.QuestionId.Text == questionModel.QuestionId);

                            List<AnswerModel> answers = new List<AnswerModel>();
                            AnswerModel answerModel = new AnswerModel();
                            answerModel.AnswerId = "";
                            answerModel.AnswerTitle = "True";
                            answerModel.AnswerCount = surveyAnswers.Count(a => a.QuestionId.Text == questionModel.QuestionId && a.AnswerTitle.Text.ToLower() == "true");
                            answerModel.AnswerPercentage = (answerModel.AnswerCount * 100) / questionModel.QuestionCount;
                            answers.Add(answerModel);

                            answerModel = new AnswerModel();
                            answerModel.AnswerId = "";
                            answerModel.AnswerTitle = "False";
                            answerModel.AnswerCount = surveyAnswers.Count(a => a.QuestionId.Text == questionModel.QuestionId && a.AnswerTitle.Text.ToLower() == "false");
                            answerModel.AnswerPercentage = (answerModel.AnswerCount * 100) / questionModel.QuestionCount;
                            answers.Add(answerModel);

                            questionModel.Answers = answers;
                            questions.Add(questionModel);
                        }
                        else
                        {
                            QuestionModel questionModel = new QuestionModel();
                            questionModel.QuestionId = question.ContentItemId;
                            questionModel.QuestionType = question.QuestionType;
                            questionModel.QuestionTitle = question.DisplayText;
                            questionModel.QuestionCount = surveyAnswers.Count(a => a.QuestionId.Text == questionModel.QuestionId);

                            List<AnswerModel> answers = new List<AnswerModel>();

                            foreach (var item in surveyAnswers.Where(a => a.QuestionId.Text == questionModel.QuestionId))
                            {
                                AnswerModel answerModel = new AnswerModel();
                                answerModel.AnswerId = item.AnswerId.Text;
                                answerModel.AnswerTitle = item.AnswerTitle.Text;
                                answerModel.AnswerCount = 1;
                                answerModel.AnswerPercentage = (answerModel.AnswerCount * 100) / questionModel.QuestionCount;
                                answers.Add(answerModel);
                            }
                            questionModel.Answers = answers;

                            questions.Add(questionModel);
                        }
                    }
                    model.Questions = questions;
                }
                //return PartialView("_SurveyResults", model);
                return model;
                //return Ok("No answers found");

            }
            catch (Exception ex)
            {
                //log ex
                return null; // StatusCode((int)System.Net.HttpStatusCode.InternalServerError);
            }
            //var productPart0 = ((Newtonsoft.Json.Linq.JProperty)((Newtonsoft.Json.Linq.JContainer)product.Content).First).Value.ToString();
            //var productPart = JsonSerializer.Deserialize<Product>(productPart0);

            // you'll get exceptions if any of these Fields are null
            // the null-conditional operator (?) should be used for any fields which aren't required
        }

        public async Task<SurveyModel> GetUserSurveyAnswerAsync(string surveyId, string userId)
        {
            try
            {
                var survey = await _orchardHelper.GetContentItemByIdAsync(surveyId);
                if (survey == null)
                {
                    return null;
                }
                var answerContentItems = await _session.Query<ContentItem, ContentItemIndex>()
                       .Where(x => x.ContentType == "SurveyAnswer" && x.Latest == true && x.Published == true)
                       .ListAsync();
                var surveyAnswer = answerContentItems.Select(x => x.As<SurveyAnswer>()).Where(a => a.SurveyId.Text == surveyId && a.UserId.Text == userId).ToList();

                List<QuestionModel> questions = new List<QuestionModel>();

                SurveyModel model = new SurveyModel();
                model.SurveyId = surveyId;
                model.UserId = userId;
                model.SurveyTitle = survey.DisplayText;

                string[] questionTypes = { "MultipleChoiceQuestion", "BooleanQuestion" };

                foreach (var question in survey.Content.BagPart.ContentItems) //Survey questions
                {
                    //if (questionTypes.Contains(item.QuestionType.Text))
                    if (question.ContentType == "MultipleChoiceQuestion") // Multiple Choice Question
                    {
                        List<AnswerModel> answers = new List<AnswerModel>();

                        QuestionModel questionModel = new QuestionModel();
                        questionModel.QuestionId = question.ContentItemId;
                        questionModel.QuestionType = question.ContentType;
                        questionModel.AllowMultipleAnswers = question.MultipleChoiceQuestion.AllowMultipleAnswers.Value;
                        questionModel.QuestionTitle = question.DisplayText;
                        //questionModel.QuestionCount = surveyAnswer.Count(a => a.QuestionId.Text == questionModel.QuestionId); //Wrong value when multiple answers
                        //int questionCount = surveyAnswers.Count(a => a.QuestionId.Text == questionModel.QuestionId);
                        foreach (var item in question.BagPart.ContentItems)
                        {
                            AnswerModel answerModel = new AnswerModel();
                            answerModel.AnswerId = item.ContentItemId;
                            answerModel.AnswerTitle = item.DisplayText;
                            answerModel.AnswerCount = surveyAnswer.Count(a => a.QuestionId.Text == questionModel.QuestionId && a.AnswerId.Text == answerModel.AnswerId);
                            //answerModel.AnswerPercentage = (answerModel.AnswerCount * 100) / questionCount;
                            answers.Add(answerModel);
                        }
                        questionModel.Answers = answers;
                        questions.Add(questionModel);
                    }
                    else if (question.ContentType == "BooleanQuestion")
                    {
                        QuestionModel questionModel = new QuestionModel();
                        questionModel.QuestionId = question.ContentItemId;
                        questionModel.QuestionType = question.ContentType;
                        questionModel.QuestionTitle = question.DisplayText;
                        //questionModel.QuestionCount = surveyAnswers.Count(a => a.QuestionId.Text == questionModel.QuestionId);

                        List<AnswerModel> answers = new List<AnswerModel>();
                        AnswerModel answerModel = new AnswerModel();
                        answerModel.AnswerId = "";
                        answerModel.AnswerTitle = "True";
                        answerModel.AnswerCount = surveyAnswer.Count(a => a.QuestionId.Text == questionModel.QuestionId && a.AnswerTitle.Text.ToLower() == "true");
                        //answerModel.AnswerPercentage = (answerModel.AnswerCount * 100) / questionModel.QuestionCount;
                        answers.Add(answerModel);

                        answerModel = new AnswerModel();
                        answerModel.AnswerId = "";
                        answerModel.AnswerTitle = "False";
                        answerModel.AnswerCount = surveyAnswer.Count(a => a.QuestionId.Text == questionModel.QuestionId && a.AnswerTitle.Text.ToLower() == "false");
                        //answerModel.AnswerPercentage = (answerModel.AnswerCount * 100) / questionModel.QuestionCount;
                        answers.Add(answerModel);

                        questionModel.Answers = answers;
                        questions.Add(questionModel);
                    }
                    else
                    {
                        QuestionModel questionModel = new QuestionModel();
                        questionModel.QuestionId = question.ContentItemId;
                        questionModel.QuestionType = question.QuestionType;
                        questionModel.QuestionTitle = question.DisplayText;
                        //questionModel.QuestionCount = surveyAnswers.Count(a => a.QuestionId.Text == questionModel.QuestionId);

                        List<AnswerModel> answers = new List<AnswerModel>();

                        foreach (var item in surveyAnswer.Where(a => a.QuestionId.Text == questionModel.QuestionId))
                        {
                            AnswerModel answerModel = new AnswerModel();
                            answerModel.AnswerId = item.AnswerId.Text;
                            answerModel.AnswerTitle = item.AnswerTitle.Text;
                            answerModel.AnswerCount = 1;
                            //answerModel.AnswerPercentage = (answerModel.AnswerCount * 100) / questionModel.QuestionCount;
                            answers.Add(answerModel);
                        }
                        questionModel.Answers = answers;

                        questions.Add(questionModel);
                    }
                }

                model.Questions = questions;
                return model;
            }
            catch (Exception ex)
            {
                //log ex
                return null; // StatusCode((int)System.Net.HttpStatusCode.InternalServerError);
            }

            //var productPart0 = ((Newtonsoft.Json.Linq.JProperty)((Newtonsoft.Json.Linq.JContainer)product.Content).First).Value.ToString();
            //var productPart = JsonSerializer.Deserialize<Product>(productPart0);

            // you'll get exceptions if any of these Fields are null
            // the null-conditional operator (?) should be used for any fields which aren't required
        }

        private List<SurveyModel> GetSurveysList()
        {
            var surveyContentItems = _session.Query<ContentItem, ContentItemIndex>()
                    .Where(x => x.ContentType == "Survey" && x.Latest == true && x.Published == true)
                    .ListAsync().Result;
            var surveys = surveyContentItems.Select(x => new SurveyModel { SurveyId = x.ContentItemId, SurveyTitle = x.DisplayText }).ToList();

            return surveys;
        }

        private List<UserModel> GetUsersList()
        {
            var answerContentItems = _session.Query<ContentItem, ContentItemIndex>()
                    .Where(x => x.ContentType == "SurveyAnswer" && x.Latest == true && x.Published == true)
                    .ListAsync().Result;
            var anonymousIds = answerContentItems.Select(x => x.As<SurveyAnswer>()).Where(a => a.IsAnonymous.Value).Select(u => u.UserId.Text).Distinct().ToList();
            var usersIds = answerContentItems.Select(x => x.As<SurveyAnswer>()).Where(a => !(a.IsAnonymous.Value)).Select(u => u.UserId.Text).Distinct().ToList();
            var usersAnswerd = _orchardHelper.GetUsersByIdsAsync(usersIds).Result;
            //var users = _orchardHelper.GetUsersByIdsAsync();
            var users = usersAnswerd.Select(x => new UserModel { UserId = x.UserId, UserName = x.UserName }).ToList();
            anonymousIds.ForEach(id => users.Add(new UserModel { UserId = id, UserName = "(Anonymous) " + id }));
            return users;
        }

    }
}
