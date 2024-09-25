
var cron1 = new CronExpression("* * * * *");
var cron2 = new CronExpression("1,2,3,4 * * * *");
var cron3 = new CronExpression("1-20 * * * *");
var cron4 = new CronExpression("5 * * * *");
var cron5 = new CronExpression("1,2,20-30 0-12 1 1,7 *");

Console.ReadKey();



public class CronExpression
{
    private readonly List<int> Minutes;
    private readonly List<int> Hours;
    private readonly List<int> DaysOfMonth;
    private readonly List<int> Months;
    private readonly List<int> DaysOfWeek;
    
    /// <summary>
    /// Creates cron expression
    /// </summary>
    /// <param name="expression">
    /// Allowed syntax:
    /// * * * * *
    /// | | | | |
    /// | | | | day of the week
    /// | | | month           
    /// | | day of the month
    /// | hour
    /// minute
    ///
    /// Values:
    /// Minutes	        0–59	* , -	
    /// Hours	        0–23	* , -	
    /// Day of month	1–31	* , -
    /// Month	        1–12    * , -	
    /// Day of week	   	1–7     * , -
    /// </param>
    public CronExpression(string expression)
    {
        var parts = expression.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 5)
            throw new ArgumentException("SYNTAX ERROR: Not all required parts was found.", nameof(expression));

        Minutes = ParsePart(parts[0], 0, 59);
        Hours = ParsePart(parts[1], 0, 23);
        DaysOfMonth = ParsePart(parts[2], 1, 31);
        Months = ParsePart(parts[3], 1, 12);
        DaysOfWeek = ParsePart(parts[4], 1, 7);
    }
    

    private List<int> ParsePart(string part, int min, int max)
    {
        var timeDefinitions =  part.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var executionTimes = new List<int>();
        
        foreach (var timeDefinition in timeDefinitions)
        {
            if (timeDefinition == "*")
                return Enumerable.Range(min, max - min + 1).ToList();

            var number = TryParseNumberWithRangeValidation(timeDefinition, min, max); 
            if (number != null)
            {
                executionTimes.Add(number.Value);
                continue;
            }

            if (timeDefinition.Contains('-'))
            {
                var rangeNums = timeDefinition.Split('-', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                if(rangeNums.Length != 2)
                    throw new ArgumentException($"SYNTAX ERROR: The range definition '{timeDefinition}' is invalid.");

                var begin = TryParseNumberWithRangeValidation(rangeNums[0], min, max);
                var end = TryParseNumberWithRangeValidation(rangeNums[1], min, max);
                if (begin == null)
                    throw new ArgumentException($"SYNTAX ERROR: The begin of the range '{rangeNums[0]}' is not a number.");
                if (end == null)
                    throw new ArgumentException($"SYNTAX ERROR: The end of the range '{rangeNums[1]}' is not a number.");

                if (end <= begin)
                    throw new AggregateException($"SYNTAX ERROR: The range '{timeDefinition}' is invalid, because the end must be greater than the begin.");
                
                executionTimes.AddRange(Enumerable.Range(begin.Value, end.Value - begin.Value + 1));
                continue;
            }

            throw new ArgumentException($"SYNTAX ERROR: Invalid time definition '{timeDefinition}'");
        }

        executionTimes.Sort();
        return executionTimes.Distinct().ToList();
    }

    private int? TryParseNumberWithRangeValidation(string str, int min, int max)
    {
        if (!int.TryParse(str, out var number))
            return null;
        
        if (!(min <= number && number <= max))
            throw new ArgumentException($"SYNTAX ERROR: A time definition was out of expected range, '{number}' is not in [{min}, {max}].");
        
        return number;

    }
}

