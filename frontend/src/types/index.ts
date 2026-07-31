export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  expiraEn: number;
  usuario: UsuarioDto;
}

export interface UsuarioDto {
  id: string;
  nombre: string;
  email: string;
  rol: 'Admin' | 'Agente' | 'Solicitante';
  tenantId: string;
  tenantNombre: string;
}

export interface CategoriaDto {
  id: string;
  nombre: string;
  slaHoras: number;
}

export interface SolicitudItem {
  id: string;
  codigo: string;
  titulo: string;
  estado: string;
  prioridad: string;
  categoria: { id: string; nombre: string };
  agente: { id: string; nombre: string } | null;
  fechaCreacion: string;
  fechaLimiteSla: string;
  vencida: boolean;
}

export interface SolicitudesResponse {
  items: SolicitudItem[];
  page: number;
  pageSize: number;
  total: number;
  totalPaginas: number;
}

export interface CrearSolicitudRequest {
  titulo: string;
  descripcion: string;
  categoriaId: string;
  prioridad: string;
}

export interface SolicitudDetalle {
  id: string;
  codigo: string;
  titulo: string;
  descripcion: string;
  estado: string;
  prioridad: string;
  categoria: { id: string; nombre: string };
  solicitante: { id: string; nombre: string };
  agente: { id: string; nombre: string } | null;
  fechaCreacion: string;
  fechaLimiteSla: string;
  fechaResolucion: string | null;
  motivoResolucion: string | null;
  motivoCancelacion: string | null;
}

export interface TransicionRequest {
  accion: string;
  agenteId?: string;
  motivo?: string;
}