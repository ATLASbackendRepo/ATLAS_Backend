using ATLAsDCL.UnitOfWork;
using ATLASDomain.DTO.Analysis;
using Dapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Data;


namespace ATLAsBLL.Services.CourseOffering.Queries.Analysis
{
    public class CourseAnalysisQueryHandler : IRequestHandler<CourseAnalysisquery, CourseAnalysisResponseDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CourseAnalysisQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }                                       

        public async Task<CourseAnalysisResponseDto> Handle(CourseAnalysisquery request, CancellationToken cancellationToken)
        {
            var response = await _unitOfWork.CourseOfferings.GetCourseAnalysisAsync(request.CourseOfferingId);

            return response;
        }
    }
}