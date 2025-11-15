namespace VideoGallery.Library;

public record YearlyStat(int? Year, int Count, DateOnly MinDate, DateOnly MaxDate, double AvgSepInDays);