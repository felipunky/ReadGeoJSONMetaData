from osgeo import gdal, ogr
import numpy as np
np.set_printoptions(threshold=np.nan)

fn = "D:\SSS\GIS\Nucleos\Nucleos_1549125498284.geojson"

ds = gdal.OpenEx( fn )
if not ds:
    raise IOError( "Could not open '%s'" %ds_fname )

numLayers = ds.GetLayerCount()

lyr = ds.GetLayer()

lyrDef = lyr.GetLayerDefn()

fldCount = lyrDef.GetFieldCount()

ftCount = lyr.GetFeatureCount()

for i in range( fldCount ):

    print( lyrDef.GetFieldDefn( i ).GetName() )

for i in range( ftCount ):

    ft = lyr.GetFeature( i )
    geo = ft.GetGeometryRef()
    idNucleo = ft.GetFieldAsDouble( 1 )
    area = ft.GetFieldAsDouble( 2 )
    length = ft.GetFieldAsDouble( 3 )

    print( "id Nucleo: " + str( idNucleo ) + ", Area: " + str( area ) + ", Length: " + str( length ) + ", Geometry: " + str( geo ) )

'''
for ft in lyr:

    geo = ft.GetGeometryRef()

    print( ft )
'''
