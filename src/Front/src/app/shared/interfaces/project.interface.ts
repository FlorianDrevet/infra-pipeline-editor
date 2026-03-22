// ─── Responses ───────────────────────────────────────────────────────────────

export interface ProjectMemberResponse {
  id: string;
  userId: string;
  entraId: string;
  role: string;
  firstName: string;
  lastName: string;
}

export interface ProjectResponse {
  id: string;
  name: string;
  description?: string;
  members: ProjectMemberResponse[];
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface CreateProjectRequest {
  name: string;
  description?: string;
}

export interface AddProjectMemberRequest {
  userId: string;
  role: string;
}

export interface UpdateProjectMemberRoleRequest {
  newRole: string;
}
