import { createRouter, createWebHistory } from 'vue-router';
import { useAuthStore } from '../stores/auth';

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/login',
      name: 'Login',
      component: () => import('../views/LoginView.vue'),
      meta: { guest: true },
    },
    {
      path: '/solicitudes',
      name: 'Solicitudes',
      component: () => import('../views/SolicitudesView.vue'),
      meta: { requiresAuth: true },
    },
    // Más rutas se agregarán después
    {
      path: '/',
      redirect: '/solicitudes',
    },
    {
      path: '/solicitudes/nueva',
      name: 'SolicitudNueva',
      component: () => import('../views/SolicitudFormView.vue'),
      meta: { requiresAuth: true },
    },
    {
      path: '/solicitudes/:id/editar',
      name: 'SolicitudEditar',
      component: () => import('../views/SolicitudFormView.vue'),
      meta: { requiresAuth: true },
    },
    {
      path: '/solicitudes/:id',
      name: 'SolicitudDetalle',
      component: () => import('../views/SolicitudDetalleView.vue'),
      meta: { requiresAuth: true },
    },
  ],
});

router.beforeEach((to, _from, next) => {
  const auth = useAuthStore();
  if (to.meta.requiresAuth && !auth.isAuthenticated) {
    next('/login');
  } else if (to.meta.guest && auth.isAuthenticated) {
    next('/solicitudes');
  } else {
    next();
  }
});

export default router;