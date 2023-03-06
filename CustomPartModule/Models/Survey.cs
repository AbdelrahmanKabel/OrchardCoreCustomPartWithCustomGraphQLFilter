using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.Media.Fields;
using System.ComponentModel.DataAnnotations;
using YesSql.Indexes;

namespace CustomPartModule.Models
{
    public class SurveyAnswer : ContentPart
    {
        public TextField SurveyId { get; set; }
        public TextField SurveyTitle { get; set; }
        public TextField UserId { get; set; }
        public TextField QuestionId { get; set; }
        public TextField QuestionType { get; set; }
        public TextField QuestionTitle { get; set; }
        public TextField AnswerId { get; set; }
        public TextField AnswerTitle { get; set; }
        public BooleanField IsAnonymous { get; set; }
    }

    public class Survey : ContentPart
    {
        public BooleanField ShowAnswers { get; set; }
    }

    public class SurveyAnswerModel
    {
        public string SurveyId { get; set; }
        public string SurveyTitle { get; set; }
        public string? UserId { get; set; }
        public List<QuestionAnswerModel> QuestionAnswers { get; set; }
    }
    public class QuestionAnswerModel
    {
        public string QuestionId { get; set; }
        public string QuestionType { get; set; }
        public string QuestionTitle { get; set; }
        public string? AnswerId { get; set; }
        public string AnswerTitle { get; set; }
    }
    public class SurveyModel
    {
        //[Required]
        public string SurveyId { get; set; }
        public string SurveyTitle { get; set; }
        public string UserId { get; set; }
        public IEnumerable<QuestionModel> Questions { get; set; }
    }
    public class QuestionModel
    {
        public string QuestionId { get; set; }
        public string QuestionType { get; set; }
        public bool AllowMultipleAnswers { get; set; }
        public string QuestionTitle { get; set; }
        public int QuestionCount { get; set; }
        public IEnumerable<AnswerModel> Answers { get; set; }
    }
    public class AnswerModel
    {
        public string AnswerId { get; set; }
        public string AnswerTitle { get; set; }
        public int AnswerCount { get; set; }
        public decimal AnswerPercentage { get; set; }
    }

    public class UserModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
    }
}
