CoordMode, Mouse, Screen

Loop, 999
{
    if !FileExist("C:/Users/jonas/Documents/Games/OMORIDEC/maps/map" A_Index ".json")
    	continue

    if FileExist("C:/Users/jonas/Documents/Games/OMORIDEC/mapexport/map" A_Index ".png")
    	continue

    Sleep, 100
    Click, 52, 1011
    Sleep, 100
    SendInput, tiled.open('../maps/map%A_Index%.json')`r
    Sleep, 400
    SendInput, for (var i = 0; i < tiled.activeAsset.layerCount; i{+}{+}) {{} var layer = tiled.activeAsset.layerAt(i); console.log(layer.name); if (layer.name === "COLLISION" || layer.name === "Collision") {{} layer.visible = false; {}} {}}`r
    Sleep, 500
    Click, 17, 31
    Sleep, 50
    Click, 94, 266
    Sleep, 50
    Click, 798, 298, 3
    Sleep, 50
    SendInput, C:/Users/jonas/Documents/Games/OMORIDEC/mapexport/map%A_Index%.png`r
    Sleep, 400
    Click, right, 300, 51
    Sleep, 50
    Click, 363, 122
    Sleep, 150
    Click, 996, 547
    Sleep, 150
}

Esc::ExitApp