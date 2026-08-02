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

const erroresLocales = ref<Record<string, string>>({});

function validarLocalmente(): boolean {
  erroresLocales.value = {};
  if (!store.titulo || store.titulo.length < 5 || store.titulo.length > 120) {
    erroresLocales.value.titulo = 'El título debe tener entre 5 y 120 caracteres.';
  }
  if (!store.descripcion || store.descripcion.length < 10 || store.descripcion.length > 4000) {
    erroresLocales.value.descripcion = 'La descripción debe tener entre 10 y 4000 caracteres.';
  }
  if (!store.categoriaId) {
    erroresLocales.value.categoriaId = 'Debe seleccionar una categoría.';
  }
  return Object.keys(erroresLocales.value).length === 0;
}

onMounted(async () => {
  await catStore.cargar();
  if (esEdicion.value && id) {
    await store.cargarParaEdicion(id);
  } else {
    store.reset();
  }
});

async function submit() {
  if (!validarLocalmente()) return;
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
        <span v-if="store.errores.titulo || erroresLocales.titulo" data-testid="error-titulo">
          {{ store.errores.titulo ? store.errores.titulo[0] : erroresLocales.titulo }}
        </span>
      </div>

      <div>
        <label>Descripción</label>
        <textarea v-model="store.descripcion" data-testid="form-descripcion"></textarea>
        <span v-if="store.errores.descripcion || erroresLocales.descripcion" data-testid="error-descripcion">
          {{ store.errores.descripcion ? store.errores.descripcion[0] : erroresLocales.descripcion }}
        </span>
      </div>

      <div>
        <label>Categoría</label>
        <select v-model="store.categoriaId" data-testid="form-categoria">
          <option value="">Seleccione una categoría</option>
          <option v-for="cat in catStore.categorias" :key="cat.id" :value="cat.id">
            {{ cat.nombre }}
          </option>
        </select>
        <span v-if="store.errores.categoriaId || erroresLocales.categoriaId" data-testid="error-categoria">
          {{ store.errores.categoriaId ? store.errores.categoriaId[0] : erroresLocales.categoriaId }}
        </span>
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