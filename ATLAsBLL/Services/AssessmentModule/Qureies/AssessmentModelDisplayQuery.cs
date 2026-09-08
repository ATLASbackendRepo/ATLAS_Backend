using ATLASDomain.DTO.AssessmentModule;
using MediatR;


namespace ATLAsBLL.Services.AssessmentModule.Qureies
{
    public record AssessmentModelDisplayQuery(int courseOfferingId) : IRequest<IEnumerable<AssessmentModelDisplayDTO>>;
   

}
