# Checklist de recuperacion

## Prioridad alta

- [x] Activar escena principal en build.
- [x] Crear GDD breve.
- [x] Crear diagrama de arquitectura.
- [x] Crear informe de optimizacion base.
- [x] Añadir salida clara de build desde `GameManager`.
- [x] Proteger guardado/audio/NPC contra referencias nulas.
- [x] Conectar por codigo los botones de reset/salir a `UIManager.RestartGame`, `UIManager.QuitGame` o `SaveMenuController.QuitGame` si el nombre del boton coincide.
- [x] Crear arranque de seguridad para audio/persistencia en la build si faltan los prefabs en la escena principal.
- [ ] Poner los 3 tipos de NPC en la escena principal.
- [x] Asignar musica de respaldo a `AdaptiveMusic` si no hay clips configurados.

## Prioridad media

- [ ] Meter al menos 6 materiales visibles en la escena.
- [ ] Añadir o activar 3 sistemas de particulas visibles: fuego, alerta, polvo/impacto.
- [ ] Configurar baked lights en objetos estaticos.
- [ ] Hacer bake de Occlusion Culling.
- [ ] Capturar Profiler y completar draw calls antes/despues.

## Prioridad baja o dificil

- [x] Personaje humanoide con Animator Controller.
- [x] Blend Tree para idle/walk/run.
- [x] IK basico en pies con raycast.
- [ ] Revisar Off-Mesh Link visible y demostrarlo con un agente.

## Frase honesta para defender la entrega

La recuperacion se ha centrado en integrar en la build final sistemas que ya existian en escenas de prueba, corregir conexiones de UI/guardado/audio, completar la documentacion obligatoria y dejar preparada la medicion de estabilidad y optimizacion.
