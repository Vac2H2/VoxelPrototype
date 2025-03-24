using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.Collections;
using Unity.Mathematics;
using Unity.PerformanceTesting;
using VoxelEngine.VoxelManager;

public class Tests
{
    // A Test behaves as an ordinary method
    [Test, Performance]
    public void FillStructureWithJob()
    {
        NativeArray<uint> voxels = new NativeArray<uint>(32 * 32, Allocator.Persistent);
        NativeSlice<uint> voxelSlice = new NativeSlice<uint>(voxels, 0, 1024);
        NativeArray<uint> structure = VoxelInstanceGeneration.CreateStructure();

        Measure.Method(() =>
        {
            VoxelInstanceGeneration.FillStructure(voxelSlice, structure);
        })
        .WarmupCount(5)           // Warmup iterations to stabilize performance
        .MeasurementCount(10)     // How many times to measure
        .IterationsPerMeasurement(1)  // How many times per measurement
        .GC() // Force garbage collection between iterations to remove noise
        .Run();

        structure.Dispose();
        voxels.Dispose();
    }

    [Test, Performance]
    public void FillStructureWithoutJob()
    {
        NativeArray<uint> voxels = new NativeArray<uint>(32 * 32, Allocator.Persistent);
        NativeArray<uint> structure = VoxelQuadsGeneration.CreateStructure();

        Measure.Method(() =>
        {
            VoxelQuadsGeneration.FillStructureWithoutJob(voxels, structure);
        })
        .WarmupCount(5)           // Warmup iterations to stabilize performance
        .MeasurementCount(10)     // How many times to measure
        .IterationsPerMeasurement(1)  // How many times per measurement
        .GC() // Force garbage collection between iterations to remove noise
        .Run();

        structure.Dispose();
        voxels.Dispose();
    }
}
