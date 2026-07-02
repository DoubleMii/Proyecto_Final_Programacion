# Recuperacion individual - Sistemas integrados

## Objetivo

La recuperacion se centra en dejar visibles e integrados los sistemas que estaban creados pero no funcionaban correctamente en build: persistencia JSON, NPCs, UI, salida de build, VFX, audio, materiales y feedback de animacion.

## Mejoras de NPC / IA

- `NPCController` se ha convertido en una FSM con estados `Patrol`, `Suspicious`, `Chase`, `Search`, `Attack` y `Return`.
- La deteccion usa distancia, campo de vision y raycast de linea de vision.
- Hay 3 roles diferenciados: `Guard`, `Hunter` y `Sentinel`.
- Cada rol ajusta velocidad, rango de deteccion, rango de ataque y duracion de busqueda.
- Si el jugador se oculta, el NPC busca en la ultima posicion conocida antes de volver a patrullar.
- `NPCBuildIntegrator` garantiza que en build haya minimo 3 NPCs activos; si faltan, crea NPCs de respaldo sobre el NavMesh.

## Mejoras audiovisuales y UI

- Los NPCs reciben material/color distinto segun rol para que los tipos se vean claramente.
- `VFXManager` genera particulas de alerta, polvo y golpe aunque no haya prefabs asignados.
- Los NPCs emiten tonos espaciales de alerta/ataque usando `AudioSource`.
- `RuntimeCharacterAnimation` alimenta un `Animator` si existe y, si no, aplica movimiento procedural visible.
- El menu de guardado conecta guardar, cargar, nueva partida, reset/borrar slot y salir.
- La UI general evita que un boton de reset del menu de guardado reinicie la escena por error.
- `GameManager.QuitGame()` permite cerrar la build y detener Play Mode en editor.

## Estructura tecnica

- `GameData`: contenedor serializable con datos de jugador, estadisticas, ajustes y metadatos del guardado.
- `SaveSystem`: lectura, escritura y borrado de archivos JSON en `Application.persistentDataPath`.
- `PersistenceManager`: punto central del sistema. Empieza cada ejecucion con partida nueva en memoria, mantiene `CurrentData`, gestiona slot activo y llama a todos los componentes que implementan `IDataPersistence`.
- `PlayerPersistence`: guarda y carga posicion, rotacion, vida, stamina, nivel y oro del jugador.
- `AudioManager`: guarda y carga volumen maestro, musica y efectos.
- `SaveMenuController` y `PersistenceUI`: conectan la UI con guardar, cargar, nueva partida y reset de slot.
- `NPCController`: comportamiento FSM de los enemigos.
- `NPCBuildIntegrator`: asegura integracion de NPCs en build.
- `VFXManager`: VFX asignados o generados en runtime.
- `RuntimeCharacterAnimation`: feedback de animacion por Animator o procedural.

## Datos persistidos

El JSON guarda:

- Posicion y rotacion del jugador.
- Vida, stamina, nivel y oro.
- Inventario como lista de nombres.
- Estadisticas de partida: tiempo jugado, muertes, enemigos eliminados, objetos recogidos y contador de guardados.
- Ajustes de audio.
- Metadatos: fecha del guardado, version, slot e indicador de guardado manual.

Ejemplo simplificado:

```json
{
  "player": {
    "posX": 4.1,
    "posY": 1.0,
    "posZ": -2.5,
    "rotY": 90.0,
    "hasSavedPosition": true,
    "health": 80.0,
    "gold": 20,
    "inventory": ["Llave"]
  },
  "stats": {
    "totalPlayTimeSeconds": 120.0,
    "enemiesKilled": 1,
    "deaths": 0,
    "itemsCollected": 1,
    "saveCount": 3
  },
  "saveSlot": 0,
  "isManualSave": true
}
```

## Slots

El sistema soporta 3 slots independientes (`0`, `1`, `2`), superando el minimo pedido de 2 slots. Cada slot usa su propio archivo:

```text
save_slot_0.json
save_slot_1.json
save_slot_2.json
```

## Cambios realizados para la recuperacion

- El juego arranca siempre desde cero, pero los slots guardados se mantienen entre ejecuciones para demostrar persistencia real.
- El guardado manual escribe JSON real en disco. El guardado automatico queda desactivado para no pisar los slots sin que el jugador pulse Guardar.
- El boton Cargar recupera el slot seleccionado solo cuando el jugador lo pulsa.
- El boton Reiniciar/Nueva partida empieza desde cero sin borrar los slots guardados.
- El boton Borrar elimina manualmente el JSON del slot seleccionado.
- La carga aplica los datos a los objetos persistentes activos en escena.
- El menu de guardado puede seleccionar slot, guardar, cargar, iniciar nueva partida, borrar/reiniciar slot y salir.
- El boton de reset/borrar elimina el JSON del slot activo y refresca los datos en memoria.
- La UI actualiza el estado visual de cada slot indicando si esta vacio o guardado.
- El gestor general de UI evita capturar los botones del menu de guardado, para que un reset de slot no reinicie la escena por error.

## Verificacion manual

1. Entrar en la build o en Play Mode usando `Assets/Scenes/FinalScene.unity`.
2. Seleccionar `Slot 1`.
3. Mover el jugador y cambiar algun dato visible, por ejemplo oro o vida.
4. Pulsar `Guardar`.
5. Mover el jugador a otra posicion.
6. Pulsar `Cargar`.
7. Comprobar que posicion, estadisticas y datos vuelven al estado guardado.
8. Repetir con `Slot 2` para verificar independencia entre slots.
9. Pulsar `Borrar slot` y comprobar que el slot queda vacio.

## Defensa tecnica

La decision principal es separar los datos puros (`GameData`) de los componentes de escena. `PersistenceManager` no necesita conocer cada sistema concreto: busca objetos con `IDataPersistence` y les pide que guarden o carguen sus datos. Asi el modulo es ampliable, porque un nuevo sistema solo debe implementar `LoadData` y `SaveData`.
