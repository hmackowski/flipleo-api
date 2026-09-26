using FlipLeo.Core.DTOs;

namespace FlipLeo.Services.Interfaces;

public interface IFlipRecordService
{
    Task<FlipRecord[]> GetFlipRecords();

    Task<FlipRecord> GetFlipRecord(int flipRecordId);

    Task<FlipRecord> AddFlipRecord(FlipRecord flipRecord);

    Task<FlipRecord> UpdateFlipRecord(FlipRecord flipRecord);

    Task<SuccessResult> DeleteFlipRecord(int flipRecordId);

    /// <summary>Adds an add-on and returns the flip with its recalculated totals.</summary>
    Task<FlipRecord> AddAddOn(int flipRecordId, FlipRecordAddOn addOn);

    /// <summary>Updates an add-on and returns the flip with its recalculated totals.</summary>
    Task<FlipRecord> UpdateAddOn(FlipRecordAddOn addOn);

    /// <summary>Deletes an add-on and returns the flip with its recalculated totals.</summary>
    Task<FlipRecord> DeleteAddOn(int addOnId);
}
