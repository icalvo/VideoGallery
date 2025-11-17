namespace VideoGallery.Interfaces;

public interface ITagValidation
{
    /// <summary>
    /// Validates a complete set of video tags.
    /// </summary>
    /// <param name="tags"></param>
    /// <returns>An error message describing the validation failure or null on success.</returns>
    string? ValidateTags(IEnumerable<ITag> tags);
    
    /// <summary>
    /// Returns a set of tag rules that can be used to determine whether a video should be tagged with a particular tag.
    /// </summary>
    /// <returns>
    /// A set of calculated tag rules.
    /// </returns>
    /// <remarks>
    /// Tag rules are applied every time a video is modified. If you change your tag rules,
    /// you will need to recalculate the calculated tags. Use the CLI to do this (vd calctags).
    ///
    /// The rules are evaluated in the order they are returned.
    ///
    /// A rule consists of a condition and a set of tags to add to the video if the condition is met.
    /// </remarks>
    IEnumerable<(Func<IVideo, bool> cond, Func<IVideo, string[]> tags)> CalculatedTagRules { get; }
    /// <summary>
    /// Title to be shown for calendar watch events associated with a video.
    /// </summary>
    /// <param name="video"></param>
    /// <returns></returns>
    string VideoEventTitle(IVideo video);
    /// <summary>
    /// Tooltip/description to be shown for calendar watch events associated with a video.
    /// </summary>
    /// <param name="video"></param>
    /// <returns></returns>
    string VideoEventTooltip(IVideo video);
}