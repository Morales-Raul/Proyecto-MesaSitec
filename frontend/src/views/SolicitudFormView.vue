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
  <div class="min-h-screen bg-[#f7f5f0] p-6 font-sans">
    <div class="max-w-2xl mx-auto space-y-4">
      
      <div>
        <button 
          type="button"
          @click="router.push('/solicitudes')"
          class="inline-flex items-center gap-1.5 text-xs font-bold text-[#204060] hover:text-[#e48338] transition-colors bg-white px-3 py-1.5 rounded border border-gray-200 shadow-sm"><span>← Regresar</span>
        </button>
      </div>

      <!-- Contenedor Principal Plano -->
      <div class="bg-white border border-gray-200 rounded shadow-sm overflow-hidden">
        
        <!-- Header -->
        <div class="p-6 border-b border-gray-200">
          <h1 class="text-2xl font-bold text-[#204060]">
            {{ esEdicion ? 'Editar' : 'Nueva' }} Solicitud
          </h1>
          <p class="text-xs text-gray-500 mt-1">
            {{ esEdicion ? 'Actualice la información del requerimiento.' : 'Complete los campos para registrar una nueva solicitud.' }}
          </p>
        </div>

        <!-- Formulario -->
        <form @submit.prevent="submit" class="p-6 space-y-5">
          
          <!-- Campo: Título -->
          <div>
            <label class="block text-xs font-semibold text-gray-700 uppercase tracking-wider mb-1.5">
              Título
            </label>
            <input 
              v-model="store.titulo" 
              data-testid="form-titulo"
              placeholder="Ingrese un título descriptivo..."
              class="w-full px-3 py-2 text-sm border rounded outline-none transition-colors text-gray-800 bg-white"
              :class="store.errores.titulo || erroresLocales.titulo ? 'border-red-500 focus:border-red-600' : 'border-gray-300 focus:border-[#204060] focus:ring-1 focus:ring-[#204060]'"
            />
            <span 
              v-if="store.errores.titulo || erroresLocales.titulo" 
              data-testid="error-titulo"
              class="block text-xs font-medium text-red-600 mt-1"
            >
              {{ store.errores.titulo ? store.errores.titulo[0] : erroresLocales.titulo }}
            </span>
          </div>

          <!-- Campo: Descripción -->
          <div>
            <label class="block text-xs font-semibold text-gray-700 uppercase tracking-wider mb-1.5">
              Descripción
            </label>
            <textarea 
              v-model="store.descripcion" 
              data-testid="form-descripcion"
              rows="5"
              placeholder="Describa en detalle su requerimiento..."
              class="w-full px-3 py-2 text-sm border rounded outline-none transition-colors text-gray-800 bg-white resize-none"
              :class="store.errores.descripcion || erroresLocales.descripcion ? 'border-red-500 focus:border-red-600' : 'border-gray-300 focus:border-[#204060] focus:ring-1 focus:ring-[#204060]'"
            ></textarea>
            <span 
              v-if="store.errores.descripcion || erroresLocales.descripcion" 
              data-testid="error-descripcion"
              class="block text-xs font-medium text-red-600 mt-1"
            >
              {{ store.errores.descripcion ? store.errores.descripcion[0] : erroresLocales.descripcion }}
            </span>
          </div>

          <!-- Grid de Selects: Categoría y Prioridad -->
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            
            <!-- Campo: Categoría -->
            <div>
              <label class="block text-xs font-semibold text-gray-700 uppercase tracking-wider mb-1.5">
                Categoría
              </label>
              <select 
                v-model="store.categoriaId" 
                data-testid="form-categoria"
                class="w-full px-3 py-2 text-sm border rounded outline-none transition-colors text-gray-800 bg-white"
                :class="store.errores.categoriaId || erroresLocales.categoriaId ? 'border-red-500 focus:border-red-600' : 'border-gray-300 focus:border-[#204060] focus:ring-1 focus:ring-[#204060]'"
              >
                <option value="">Seleccione una categoría</option>
                <option v-for="cat in catStore.categorias" :key="cat.id" :value="cat.id">
                  {{ cat.nombre }}
                </option>
              </select>
              <span 
                v-if="store.errores.categoriaId || erroresLocales.categoriaId" 
                data-testid="error-categoria"
                class="block text-xs font-medium text-red-600 mt-1"
              >
                {{ store.errores.categoriaId ? store.errores.categoriaId[0] : erroresLocales.categoriaId }}
              </span>
            </div>

            <!-- Campo: Prioridad -->
            <div>
              <label class="block text-xs font-semibold text-gray-700 uppercase tracking-wider mb-1.5">
                Prioridad
              </label>
              <select 
                v-model="store.prioridad" 
                data-testid="form-prioridad"
                class="w-full px-3 py-2 text-sm border border-gray-300 rounded outline-none focus:border-[#204060] focus:ring-1 focus:ring-[#204060] text-gray-800 bg-white"
              >
                <option value="Baja">Baja</option>
                <option value="Media">Media</option>
                <option value="Alta">Alta</option>
                <option value="Critica">Crítica</option>
              </select>
            </div>

          </div>

          <!-- Error General del Store -->
          <div 
            v-if="store.error" 
            class="bg-red-50 border border-red-200 text-red-700 p-3 rounded text-xs font-medium"
          >
            {{ store.error }}
          </div>

          <!-- Botones de Acción -->
          <div class="flex items-center justify-end gap-3 pt-4 border-t border-gray-100">
            <button 
              type="button" 
              @click="cancelar" 
              data-testid="form-cancelar"
              class="px-4 py-2 border border-gray-300 rounded text-gray-700 font-medium text-sm bg-white hover:bg-gray-50 transition-colors"
            >
              Cancelar
            </button>
            <button 
              type="submit" 
              data-testid="form-submit" 
              :disabled="store.loading"
              class="px-4 py-2 bg-[#204060] hover:bg-[#152a40] text-white font-medium text-sm rounded transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {{ esEdicion ? 'Guardar cambios' : 'Crear solicitud' }}
            </button>
          </div>

        </form>
      </div>

    </div>
  </div>
</template>