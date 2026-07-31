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