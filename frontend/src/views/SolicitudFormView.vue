<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useSolicitudFormStore } from '../stores/solicitudForm';
import { useCategoriasStore } from '../stores/categorias';

const route = useRoute();
const router = useRouter();
const store = useSolicitudFormStore();
const catStore = useCategoriasStore();

const esEdicion = computed(() => !!route.params.id);
const id = route.params.id as string | undefined;
const errorGeneral = ref('');

onMounted(async () => {
  await catStore.cargar();
  if (esEdicion.value && id) {
    await store.cargarParaEdicion(id);
  } else {
    store.reset();
  }
});

async function submit() {
  errorGeneral.value = '';
  const result = esEdicion.value
    ? await store.editar(id!)
    : await store.crear();

  if (result) {
    router.push(`/solicitudes/${result.id}`);
  }
}

function cancelar() {
  router.back();
}
</script>

<template>
  <div>
    <h1>{{ esEdicion ? 'Editar' : 'Nueva' }} Solicitud</h1>

    <form @submit.prevent="submit">
      <div>
        <label>Título</label>
        <input v-model="store.titulo" data-testid="form-titulo" />
        <span v-if="store.errores.titulo" data-testid="error-titulo">{{ store.errores.titulo[0] }}</span>
      </div>

      <div>
        <label>Descripción</label>
        <textarea v-model="store.descripcion" data-testid="form-descripcion"></textarea>
        <span v-if="store.errores.descripcion" data-testid="error-descripcion">{{ store.errores.descripcion[0] }}</span>
      </div>

      <div>
        <label>Categoría</label>
        <select v-model="store.categoriaId" data-testid="form-categoria">
          <option value="">Seleccione una categoría</option>
          <option v-for="cat in catStore.categorias" :key="cat.id" :value="cat.id">
            {{ cat.nombre }}
          </option>
        </select>
        <span v-if="store.errores.categoriaId" data-testid="error-categoria">{{ store.errores.categoriaId[0] }}</span>
      </div>

      <div>
        <label>Prioridad</label>
        <select v-model="store.prioridad" data-testid="form-prioridad">
          <option value="Baja">Baja</option>
          <option value="Media">Media</option>
          <option value="Alta">Alta</option>
          <option value="Critica">Crítica</option>
        </select>
      </div>

      <p v-if="store.error">{{ store.error }}</p>

      <button type="submit" data-testid="form-submit" :disabled="store.loading">
        {{ esEdicion ? 'Guardar cambios' : 'Crear solicitud' }}
      </button>
      <button type="button" @click="cancelar" data-testid="form-cancelar">Cancelar</button>
    </form>
  </div>
</template>