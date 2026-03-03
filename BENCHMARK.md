# CAD to PDF Benchmark Report

Generated: 2026-03-03 18:41  
Iterations per converter: 3  
Mode: Release

## Results

| Sample | Converter | Success | Elapsed | CPU | Peak Memory | Memory Delta | Output PDF Size |
|---|---|:---:|---:|---:|---:|---:|---:|
| fish_processing_plant.dwg | CadLib (WoutWare) | OK | 78739.81 ms | 73843.75 ms | 561.8 MB | 499.3 MB | 11900.8 KB |
| fish_processing_plant.dwg | Aspose.CAD | OK | 214715.25 ms | 228015.62 ms | 884.8 MB | 704.3 MB | 94222.6 KB |
| home_floor_plan.dwg | CadLib (WoutWare) | OK | 142182.97 ms | 137437.50 ms | 391.4 MB | 312.6 MB | 3388.6 KB |
| home_floor_plan.dwg | Aspose.CAD | OK | 63843.42 ms | 71484.38 ms | 465.3 MB | 348 MB | 18712.8 KB |
| MLC650.dwg | CadLib (WoutWare) | OK | 11946.20 ms | 7921.88 ms | 191.5 MB | 129.1 MB | 541.7 KB |
| MLC650.dwg | Aspose.CAD | OK | 18076.67 ms | 24875.00 ms | 251.3 MB | 142.2 MB | 4087.7 KB |
| new-york.dxf | CadLib (WoutWare) | OK | 46394.93 ms | 43546.88 ms | 764 MB | 692.8 MB | 10638.9 KB |
| new-york.dxf | Aspose.CAD | OK | 160392.07 ms | 161562.50 ms | 437.8 MB | 203.7 MB | 13580.1 KB |
| visualization_-_condominium_with_skylight.dwg | CadLib (WoutWare) | OK | 7633.87 ms | 3625.00 ms | 158.4 MB | 96.4 MB | 10.1 KB |
| visualization_-_condominium_with_skylight.dwg | Aspose.CAD | OK | 20858.76 ms | 25578.12 ms | 242.1 MB | 136.3 MB | 4998.8 KB |

## PDF Fidelity Notes

Automated pixel-diff fidelity is not implemented. Inspect outputs manually:

```
benchmark/<sample>/CadLib_WoutWare/output_0.pdf
benchmark/<sample>/Aspose.CAD/output_0.pdf
```

### Suggested fidelity checklist
- [ ] No missing text / glyphs
- [ ] Correct layout page orientation and paper size
- [ ] Similar drawing extents and scale
- [ ] Lineweights and linetypes preserved
- [ ] Hatches and fills rendered
- [ ] Viewports and clipping correct (paper-space layouts)
- [ ] No major geometry omissions