# Informe de optimizacion

## Objetivo

Mantener la build estable por encima de 30 FPS y reducir coste de renderizado en la escena principal `Assets/Scenes/01_Main_Level.unity`.

## Ajustes aplicados o preparados

- Escena principal activada en `EditorBuildSettings` para asegurar que la build usa el nivel correcto.
- Sistema `BuildStabilityChecker` disponible para medir FPS, errores y warnings durante ejecucion.
- Uso de `NavMesh` para evitar calculos manuales de movimiento de NPCs.
- Uso de prefabs centralizados para audio, persistencia y VFX.
- Referencias de audio y UI protegidas contra nulos para evitar errores en build.
- Boton de salida integrado mediante `GameManager.QuitGame()`.

## Occlusion Culling

Pendiente de captura en Unity:

1. Abrir `Window > Rendering > Occlusion Culling`.
2. Marcar como `Static` los objetos grandes del escenario que no se mueven.
3. En la ventana de Occlusion Culling, pulsar `Bake`.
4. Activar la visualizacion para comprobar que las zonas no visibles se descartan.
5. Guardar captura del panel y de la escena.

## Profiler

Pendiente de captura en Unity:

1. Abrir `Window > Analysis > Profiler`.
2. Ejecutar la escena durante al menos 30 segundos.
3. Capturar los modulos `CPU Usage`, `Rendering` y `Memory`.
4. Anotar FPS medio, FPS minimo y draw calls.

## Tabla antes/despues

Completar con los datos reales del Profiler:

| Medida | Antes | Despues | Comentario |
| --- | ---: | ---: | --- |
| FPS medio | Pendiente | Pendiente | Medir en build o Play Mode |
| FPS minimo | Pendiente | Pendiente | Debe mantenerse sobre 30 FPS |
| Draw Calls | Pendiente | Pendiente | Revisar modulo Rendering |
| Batches | Pendiente | Pendiente | Mejorar con batching/materiales compartidos |
| Errores de consola | 0 | 0 | Revisar consola de Unity y build |

## Comentario final

La optimizacion principal pendiente es documentar con capturas reales el Profiler y el Occlusion Culling. La escena ya cuenta con sistemas preparados para medir estabilidad y se han reducido riesgos de errores en build por referencias no asignadas.
