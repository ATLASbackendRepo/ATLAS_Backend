using MediatR;
using System.Collections.Generic;
using ATLASDomain.DTO.Analysis;

namespace ATLAsBLL.Services.CourseOffering.Queries.Analysis
{
    /// <summary>
    /// Course Analysisquery.
    /// </summary>
    public record CourseAnalysisquery(int CourseOfferingId) : IQuery<CourseAnalysisResponseDto>;
   
}
