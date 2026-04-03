using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRManagement.Tests.Helplers
{
    //public class CalculateEffectiveLeaveDaysTests
    //{
    //}

    using FluentAssertions;
    using global::HRManagement.Helpers;
    //using HRManagement.Helpers;
    using System;
    using Xunit;

    namespace HRManagement.Tests.Helpers
    {
        public class CalculateEffectiveLeaveDaysTests
        {
            // -------- SAME DAY TESTS --------

            [Fact]
            public void SameDay_FullDay_ShouldReturnOne()
            {
                var date = new DateOnly(2025, 10, 15); // Wednesday

                var result = CalculateEffectiveLeaveDays.GetEffectiveLeaveDays(
                    date, date, false, false);

                result.Should().Be(1m);
            }

            [Fact]
            public void SameDay_HalfDay_ShouldReturnPointFive()
            {
                var date = new DateOnly(2025, 10, 15);

                var result = CalculateEffectiveLeaveDays.GetEffectiveLeaveDays(
                    date, date, true, false);

                result.Should().Be(0.5m);
            }

            [Fact]
            public void SameDay_Weekend_ShouldReturnZero()
            {
                var date = new DateOnly(2025, 10, 18); // Saturday

                var result = CalculateEffectiveLeaveDays.GetEffectiveLeaveDays(
                    date, date, false, false);

                result.Should().Be(0m);
            }

            // -------- MULTI-DAY TESTS --------

            [Fact]
            public void MultiDay_NoWeekends_FullDays_ShouldReturnCorrectCount()
            {
                var start = new DateOnly(2025, 10, 13); // Monday
                var end = new DateOnly(2025, 10, 15);   // Wednesday

                var result = CalculateEffectiveLeaveDays.GetEffectiveLeaveDays(
                    start, end, false, false);

                // Mon = 1, Tue = 1, Wed = 1 → Total = 3
                result.Should().Be(3m);
            }

            [Fact]
            public void MultiDay_WithWeekend_ShouldSkipWeekend()
            {
                var start = new DateOnly(2025, 10, 17); // Friday
                var end = new DateOnly(2025, 10, 20);   // Monday

                var result = CalculateEffectiveLeaveDays.GetEffectiveLeaveDays(
                    start, end, false, false);

                // Fri = 1, Sat/Sun skipped, Mon = 1 → 2 days
                result.Should().Be(2m);
            }

            [Fact]
            public void MultiDay_StartHalfDay_ShouldCalculateProperly()
            {
                var start = new DateOnly(2025, 10, 13); // Monday
                var end = new DateOnly(2025, 10, 14);   // Tuesday

                var result = CalculateEffectiveLeaveDays.GetEffectiveLeaveDays(
                    start, end, true, false);

                // Mon = 0.5, Tue = 1 → Total = 1.5
                result.Should().Be(1.5m);
            }

            [Fact]
            public void MultiDay_EndHalfDay_ShouldCalculateProperly()
            {
                var start = new DateOnly(2025, 10, 13);
                var end = new DateOnly(2025, 10, 14);

                var result = CalculateEffectiveLeaveDays.GetEffectiveLeaveDays(
                    start, end, false, true);

                // Mon = 1, Tue = 0.5 → Total = 1.5
                result.Should().Be(1.5m);
            }

            [Fact]
            public void MultiDay_BothHalfDays_ShouldCalculateProperly()
            {
                var start = new DateOnly(2025, 10, 13);
                var end = new DateOnly(2025, 10, 14);

                var result = CalculateEffectiveLeaveDays.GetEffectiveLeaveDays(
                    start, end, true, true);

                // Mon = 0.5, Tue = 0.5 → Total = 1
                result.Should().Be(1m);
            }

            // -------- INTERNAL METHOD TESTS --------

            [Fact]
            public void IsWeekendOrPublicHoliday_Saturday_ShouldReturnTrue()
            {
                var date = new DateOnly(2025, 10, 18); // Saturday

                var result = CalculateEffectiveLeaveDays.IsWeekendOrPublicHoliday(date);

                result.Should().BeTrue();
            }

            [Fact]
            public void IsWeekendOrPublicHoliday_Weekday_ShouldReturnFalse()
            {
                var date = new DateOnly(2025, 10, 15); // Wednesday

                var result = CalculateEffectiveLeaveDays.IsWeekendOrPublicHoliday(date);

                result.Should().BeFalse();
            }
        }
    }

}
