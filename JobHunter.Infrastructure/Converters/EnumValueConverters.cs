using System;
using JobHunter.Domain.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace JobHunter.Infrastructure.Converters
{
    public class LevelEnumValueConverter : ValueConverter<Level, string>
    {
        public LevelEnumValueConverter() : base(
            v => v.ToString().ToUpper(),
            v => Enum.Parse<Level>(v, true))
        { }
    }

    public class GenderEnumValueConverter : ValueConverter<Gender, string>
    {
        public GenderEnumValueConverter() : base(
            v => v.ToString().ToUpper(),
            v => Enum.Parse<Gender>(v, true))
        { }
    }

    public class ResumeStateEnumValueConverter : ValueConverter<ResumeState, string>
    {
        public ResumeStateEnumValueConverter() : base(
            v => v.ToString().ToUpper(),
            v => Enum.Parse<ResumeState>(v, true))
        { }
    }
}
