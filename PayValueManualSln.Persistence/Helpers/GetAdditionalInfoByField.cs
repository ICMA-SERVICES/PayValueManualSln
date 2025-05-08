using PayValueManualSln.Application.DTOs.Assesment;
using PayValueManualSln.Application.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Persistence.Helpers
{
    public static class BillHelper
    {
        public static List<BillAdditionalInfoDto> GetAdditionalInfoByField(
            List<BillAdditionalInfoDto> additionalInfos,
            AdditionalServiceDetailDefinitions field)
        {
            var fieldName = field.ToDescription().Trim().ToLower();
            return additionalInfos
                .Where(x => x.FieldName?.Trim().ToLower() == fieldName)
                .ToList();
        }
    }
}
