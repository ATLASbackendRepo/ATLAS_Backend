using MediatR;
using System.Collections.Generic;
using ATLASDomain.DTO.Analysis;

namespace ATLAsBLL.Services.CourseOffering.Queries.Analysis
{
    public record CourseAnalysisquery(int CourseOfferingId) : IQuery<CourseAnalysisResponseDto>;
   
}
