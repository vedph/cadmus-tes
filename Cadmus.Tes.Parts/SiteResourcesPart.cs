using System.Collections.Generic;
using System.Text;
using Cadmus.Core;
using Fusi.Tools.Configuration;

namespace Cadmus.Tes.Parts;

/// <summary>
/// Site resources part.
/// <para>Tag: <c>it.vedph.tes.site-resources</c>.</para>
/// </summary>
[Tag("it.vedph.tes.site-resources")]
public sealed class SiteResourcesPart : PartBase
{
    /// <summary>
    /// Gets or sets the resources.
    /// </summary>
    public List<SiteResource> Resources { get; set; } = [];

    /// <summary>
    /// Get all the key=value pairs (pins) exposed by the implementor.
    /// </summary>
    /// <param name="item">The optional item. The item with its parts
    /// can optionally be passed to this method for those parts requiring
    /// to access further data.</param>
    /// <returns>The pins: <c>tot-count</c> and a collection of pins.</returns>
    public override IEnumerable<DataPin> GetDataPins(IItem? item = null)
    {
        DataPinBuilder builder = new();

        builder.Set("tot", Resources?.Count ?? 0, false);

        if (Resources?.Count > 0)
        {
            HashSet<string> eids = [], types = [], features = [],
                locEids = [], locLabels = [];
            HashSet<double> dateValues = [];

            foreach (SiteResource entry in Resources)
            {
                if (!string.IsNullOrEmpty(entry.Eid)) eids.Add(entry.Eid);
                types.Add(entry.Type);

                if (entry.Features?.Count > 0)
                {
                    foreach (string feature in entry.Features)
                        features.Add(feature);
                }

                if (entry.Location != null)
                {
                    locLabels.Add(entry.Location.Value.Label);
                    if (!string.IsNullOrEmpty(entry.Location.Value.Eid))
                        locEids.Add(entry.Location.Value.Eid);
                }

                if (entry.Dates is not null)
                {
                    foreach (var date in entry.Dates)
                        dateValues.Add(date.GetSortValue());
                }
            }

            builder.AddValues("eid", eids);
            builder.AddValues("type", types);
            builder.AddValues("feature", features);
            builder.AddValues("loc-eid", locEids);
            builder.AddValues("loc-label", locLabels);
            foreach (double dateValue in dateValues)
                builder.AddValue("date-value", dateValue);
        }

        return builder.Build(this);
    }

    /// <summary>
    /// Gets the definitions of data pins used by the implementor.
    /// </summary>
    /// <returns>Data pins definitions.</returns>
    public override IList<DataPinDefinition> GetDataPinDefinitions()
    {
        return new List<DataPinDefinition>(
        [
            new DataPinDefinition(DataPinValueType.Integer,
               "tot-count",
               "The total count of entries."),
            new DataPinDefinition(DataPinValueType.String,
               "eid",
               "The EIDs of site resources.",
               "M"),
            new DataPinDefinition(DataPinValueType.String,
               "type",
               "The distinct types of site resources.",
               "M"),
            new DataPinDefinition(DataPinValueType.String,
               "feature",
               "The distinct features of site resources.",
               "M"),
            new DataPinDefinition(DataPinValueType.String,
                "loc-eid",
                "The EIDs of the locations of site resources.",
                "M"),
            new DataPinDefinition(DataPinValueType.String,
                "loc-label",
                "The distinct labels of the locations of site resources.",
                "M"),
            new DataPinDefinition(DataPinValueType.Decimal,
                "date-value",
                "The distinct date values of site resources.",
                "M")
        ]);
    }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        StringBuilder sb = new();

        sb.Append("[SiteResource]");

        if (Resources?.Count > 0)
        {
            sb.Append(' ');
            int n = 0;
            foreach (var entry in Resources)
            {
                if (++n > 3) break;
                if (n > 1) sb.Append("; ");
                sb.Append(entry);
            }
            if (Resources.Count > 3)
                sb.Append("...(").Append(Resources.Count).Append(')');
        }

        return sb.ToString();
    }
}
