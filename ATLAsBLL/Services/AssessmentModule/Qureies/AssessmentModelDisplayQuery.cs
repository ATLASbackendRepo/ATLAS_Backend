using ATLASDomain.DTO.AssessmentModule;
using MediatR;


namespace ATLAsBLL.Services.AssessmentModule.Qureies
{
    /// <summary>
    /// Assessment Model Display Query.
    /// </summary>
    public record AssessmentModelDisplayQuery(int courseOfferingId) : IRequest<IEnumerable<AssessmentModelDisplayDTO>>;
   

}
