using ATLAsDCL.UnitOfWork;
using ATLASDomain.DTO.Analysis;
using Dapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Data;


namespace ATLAsBLL.Services.CourseOffering.Queries.Analysis
{
    /// <summary>
    /// Course Analysis Query Handler.
    /// </summary>
    public class CourseAnalysisQueryHandler : IRequestHandler<CourseAnalysisquery, CourseAnalysisResponseDto>
    {
        /// <summary>
        /// _unit Of Work.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        public CourseAnalysisQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }                                       

        /// <summary>
        /// Handle.
        /// </summary>
        public async Task<CourseAnalysisResponseDto> Handle(CourseAnalysisquery request, CancellationToken cancellationToken)
        {
            var response = await _unitOfWork.CourseOfferings.GetCourseAnalysisAsync(request.CourseOfferingId);

            return response;
        }
    }
}
