using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.Collections;
using Unity.Mathematics;
using Unity.PerformanceTesting;
using VoxelEngine.VoxelManager;
using Unity.Burst;
using Unity.Jobs;

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

    [Test, Performance]
    public void FillSwizzledWithJob()
    {
        NativeArray<uint> voxels = new NativeArray<uint>(32 * 32, Allocator.Persistent);
        NativeArray<uint> structure = VoxelQuadsGeneration.CreateStructure();
        NativeArray<uint> swizzledStructure = VoxelQuadsGeneration.CreateStructure();
        VoxelQuadsGeneration.FillStructureWithoutJob(voxels, structure);

        Measure.Method(() =>
        {
            VoxelInstanceGeneration.FillSwizzledStructure(structure, swizzledStructure);
        })
        .WarmupCount(5)           // Warmup iterations to stabilize performance
        .MeasurementCount(10)     // How many times to measure
        .IterationsPerMeasurement(1)  // How many times per measurement
        .GC() // Force garbage collection between iterations to remove noise
        .Run();

        structure.Dispose();
        voxels.Dispose();
        swizzledStructure.Dispose();
    }

    [Test, Performance]
    public void FillSwizzledWithoutJob()
    {
        NativeArray<uint> voxels = new NativeArray<uint>(32 * 32, Allocator.Persistent);
        NativeArray<uint> structure = VoxelQuadsGeneration.CreateStructure();
        NativeArray<uint> swizzledStructure = VoxelQuadsGeneration.CreateStructure();
        VoxelQuadsGeneration.FillStructureWithoutJob(voxels, structure);

        Measure.Method(() =>
        {
            VoxelQuadsGeneration.FillSwizzledStructureWithoutJob(structure, swizzledStructure);
        })
        .WarmupCount(5)           // Warmup iterations to stabilize performance
        .MeasurementCount(10)     // How many times to measure
        .IterationsPerMeasurement(1)  // How many times per measurement
        .GC() // Force garbage collection between iterations to remove noise
        .Run();

        structure.Dispose();
        voxels.Dispose();
        swizzledStructure.Dispose();
    }

    [Test, Performance]
    public void GenerateGreedyQuadsWithJob()
    {
        NativeArray<uint> voxels = new NativeArray<uint>(32 * 32, Allocator.Persistent);
        NativeArray<uint> structure = VoxelQuadsGeneration.CreateStructure();
        NativeArray<uint> swizzledStructure = VoxelQuadsGeneration.CreateStructure();
        VoxelQuadsGeneration.FillStructureWithoutJob(voxels, structure);
        VoxelInstanceGeneration.FillSwizzledStructure(structure, swizzledStructure);

        Measure.Method(() =>
        {
            NativeList<VoxelInstanceManager.InstanceData> instances
            = VoxelInstanceGeneration.GenerateGreedyQuads(swizzledStructure);
            instances.Dispose();
        })
        .WarmupCount(5)           // Warmup iterations to stabilize performance
        .MeasurementCount(10)     // How many times to measure
        .IterationsPerMeasurement(1)  // How many times per measurement
        .GC() // Force garbage collection between iterations to remove noise
        .Run();

        structure.Dispose();
        voxels.Dispose();
        swizzledStructure.Dispose();
    }

    [Test, Performance]
    public void GenerateGreedyQuadsWithoutJob()
    {
        NativeArray<uint> voxels = new NativeArray<uint>(32 * 32, Allocator.Persistent);
        NativeArray<uint> structure = VoxelQuadsGeneration.CreateStructure();
        NativeArray<uint> swizzledStructure = VoxelQuadsGeneration.CreateStructure();
        VoxelQuadsGeneration.FillStructureWithoutJob(voxels, structure);
        VoxelQuadsGeneration.FillSwizzledStructureWithoutJob(structure, swizzledStructure);

        Measure.Method(() =>
        {
            NativeList<VoxelQuadsGeneration.InstanceData> instances
            = VoxelQuadsGeneration.GenerateGreedyQuads(swizzledStructure);
            instances.Dispose();
        })
        .WarmupCount(5)           // Warmup iterations to stabilize performance
        .MeasurementCount(10)     // How many times to measure
        .IterationsPerMeasurement(1)  // How many times per measurement
        .GC() // Force garbage collection between iterations to remove noise
        .Run();

        structure.Dispose();
        voxels.Dispose();
        swizzledStructure.Dispose();
    }

    [Test, Performance]
    public void PipelineWithJob()
    {
        NativeArray<uint> voxels = new NativeArray<uint>(1024, Allocator.Persistent);
        for (int i = 0; i < 512; i++)
        {
            VoxelQuadsGeneration.AddVoxel(
                voxels,
                UnityEngine.Random.Range(0, 32),
                UnityEngine.Random.Range(0, 32),
                UnityEngine.Random.Range(0, 32)
            );
        }
        NativeSlice<uint> voxelSlice = new NativeSlice<uint>(voxels, 0, 1024);

        Measure.Method(() =>
        {
            NativeList<VoxelInstanceManager.InstanceData> instances
            = VoxelInstanceGeneration.InstancePipeline(voxelSlice);

            instances.Dispose();
        })

        .WarmupCount(5)           // Warmup iterations to stabilize performance
        .MeasurementCount(10)     // How many times to measure
        .IterationsPerMeasurement(1)  // How many times per measurement
        .GC() // Force garbage collection between iterations to remove noise
        .Run();

        voxels.Dispose();
    }

    [Test, Performance]
    public void PipelineWithoutJob()
    {
        NativeArray<uint> voxels = new NativeArray<uint>(1024, Allocator.Persistent);
        for (int i = 0; i < 512; i++)
        {
            VoxelQuadsGeneration.AddVoxel(
                voxels,
                UnityEngine.Random.Range(0, 32),
                UnityEngine.Random.Range(0, 32),
                UnityEngine.Random.Range(0, 32)
            );
        }
        Measure.Method(() =>
        {
            NativeList<VoxelQuadsGeneration.InstanceData> instances
            = VoxelQuadsGeneration.GreedyQuadsPipeline(voxels);

            instances.Dispose();
        })

        .WarmupCount(5)           // Warmup iterations to stabilize performance
        .MeasurementCount(10)     // How many times to measure
        .IterationsPerMeasurement(1)  // How many times per measurement
        .GC() // Force garbage collection between iterations to remove noise
        .Run();

        voxels.Dispose();
    }
}
