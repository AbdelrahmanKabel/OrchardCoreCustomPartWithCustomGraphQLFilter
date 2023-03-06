//using GraphQL;
using Microsoft.AspNetCore.Mvc;
using CustomPartModule.Models;
using OrchardCore;
using System.Text.Json;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;

using System.Linq;
using OrchardCore.ContentFields.Fields;
using Microsoft.AspNetCore.Authorization;
using OrchardCore.Title.Models;
using YesSql;
using ISession = YesSql.ISession;
using OrchardCore.Users.Indexes;

namespace CustomPartModule.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]
    public class SurveyController : Controller
    {
        private readonly IOrchardHelper _orchardHelper;
        private readonly IContentManager _contentManager;
        private readonly ISession _session;


        public SurveyController(IContentManager contentManager, ISession session, IOrchardHelper orchardHelper)
        {
            _contentManager = contentManager;
            _session = session;
            _orchardHelper = orchardHelper;
        }
        [HttpGet("/api/getsurveyslist")]
        public async Task<ActionResult> GetSurveysListAsync()
        {
            try
            {
                var surveyContentItems = await _session.Query<ContentItem, ContentItemIndex>()
                       .Where(x => x.ContentType == "Survey" && x.Latest == true && x.Published == true)
                       .ListAsync();
                var surveys = surveyContentItems.Select(x => new SurveyModel { SurveyId = x.ContentItemId, SurveyTitle = x.DisplayText }).ToList();
                var users = await _session.Query<ContentItem, UserIndex>().ListAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                //log ex
                return StatusCode((int)System.Net.HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost("/api/surveyanswers")]
        public async Task<IActionResult> CreateSurveyAnswer([FromBody] SurveyAnswerModel model)
        {
            try
            {
                if(model != null && model.SurveyId != null)
                {
                    var answerContentItems = await _session.Query<ContentItem, ContentItemIndex>()
                       .Where(x => x.ContentType == "SurveyAnswer" && x.Latest == true && x.Published == true)
                       .ListAsync();
                    var surveyAnswers = answerContentItems.Select(x => x.As<SurveyAnswer>()).Where(a => a.SurveyId.Text == model.SurveyId);

                    if (!string.IsNullOrEmpty(model.UserId) && surveyAnswers.Any(a => a.UserId.Text == model.UserId))
                    {
                        return Ok("You answered before!");
                    }
                    else
                    {
                        bool isAnonymous = false;
                        if (string.IsNullOrEmpty(model.UserId))
                        {
                            isAnonymous = true;
                            model.UserId = Guid.NewGuid().ToString();
                        }
                        foreach (var item in model.QuestionAnswers)
                        {
                            var contentItem = await _contentManager.NewAsync("SurveyAnswer");
                            contentItem.Weld<SurveyAnswer>();
                            contentItem.Alter<SurveyAnswer>(a =>
                            {
                                a.SurveyId = new TextField { Text = model.SurveyId };
                                a.SurveyTitle = new TextField { Text = model.SurveyTitle };
                                a.UserId = new TextField { Text = model.UserId };
                                a.QuestionId = new TextField { Text = item.QuestionId };
                                a.QuestionType = new TextField { Text = item.QuestionType };
                                a.QuestionTitle = new TextField { Text = item.QuestionTitle };
                                a.AnswerId = new TextField { Text = item.AnswerId };
                                a.AnswerTitle = new TextField { Text = item.AnswerTitle };
                                a.IsAnonymous = new BooleanField { Value = isAnonymous };
                            });
                            var titlePart = contentItem.As<TitlePart>();
                            titlePart.Title = model.SurveyTitle + "_" + item.QuestionTitle + "_" + model.UserId;
                            contentItem.DisplayText = titlePart.Title; // Text box for a title part in an admin panel read a value from DisplayText
                            titlePart.Apply(); // We need to explicitly call Apply to apply a change to content item

                            await _contentManager.CreateAsync(contentItem, VersionOptions.Published);
                        }
                    }
                }


                return StatusCode((int)System.Net.HttpStatusCode.Created); // "Done";
            }
            catch (Exception ex)
            {
                //log ex
                return StatusCode((int)System.Net.HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet("/api/surveyanswers/{surveyId}")]
        public async Task<ActionResult> GetSurveyAnswersAsync(string surveyId)
        {
            try
            {
                var survey = await _orchardHelper.GetContentItemByIdAsync(surveyId);
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
                                answerModel.AnswerPercentage =  (answerModel.AnswerCount * 100) / questionCount;
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
                    return Ok(model);
                }

                return Ok("No answers found");

            }
            catch (Exception ex)
            {
                //log ex
                return StatusCode((int)System.Net.HttpStatusCode.InternalServerError);
            }
            //var productPart0 = ((Newtonsoft.Json.Linq.JProperty)((Newtonsoft.Json.Linq.JContainer)product.Content).First).Value.ToString();
            //var productPart = JsonSerializer.Deserialize<Product>(productPart0);

            // you'll get exceptions if any of these Fields are null
            // the null-conditional operator (?) should be used for any fields which aren't required
        }
        [HttpGet("/api/usersurveyanswer/{surveyId}/{userId}")]
        public async Task<ActionResult> GetUserSurveyAnswerAsync(string surveyId, string userId)
        {
            try
            {
                var survey = await _orchardHelper.GetContentItemByIdAsync(surveyId);
                var surveyObject = survey.As<Survey>();
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
                return Ok(model);
            }
            catch (Exception ex)
            {
                //log ex
                return StatusCode((int)System.Net.HttpStatusCode.InternalServerError);
            }

            //var productPart0 = ((Newtonsoft.Json.Linq.JProperty)((Newtonsoft.Json.Linq.JContainer)product.Content).First).Value.ToString();
            //var productPart = JsonSerializer.Deserialize<Product>(productPart0);

            // you'll get exceptions if any of these Fields are null
            // the null-conditional operator (?) should be used for any fields which aren't required
        }
    }
}
