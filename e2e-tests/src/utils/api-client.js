const axios = require('axios');


class ApiClient {
  constructor(baseURL = process.env.API_GATEWAY_URL || 'http://localhost:443') {
    this.client = axios.create({
      baseURL,
      timeout: 30000,
      headers: {
        'Content-Type': 'application/json'
      }
    });

    this.client.interceptors.request.use(
      (config) => {
        console.log(`🔄 ${config.method?.toUpperCase()} ${config.url}`);
        return config;
      },
      (error) => {
        console.error('Error:', error.message);
        return Promise.reject(error);
      }
    );

    this.client.interceptors.response.use(
      (response) => {
        console.log(`✅ ${response.status} ${response.config.method?.toUpperCase()} ${response.config.url}`);
        return response;
      },
      (error) => {
        const status = error.response?.status || 'Desconocido';
        const method = error.config?.method?.toUpperCase() || 'Desconocido';
        const url = error.config?.url || 'Desconocido';
        console.error(`${status} ${method} ${url}`);
        return Promise.reject(error);
      }
    );
    this.authToken = null;
  }

  setAuthToken(token) {
    this.authToken = token;
    this.client.defaults.headers.common['Authorization'] = `Bearer ${token}`;
  }

  async login(email, password) {
    try {
      const response = await this.client.post('/auth/login', {
        email,
        password
      });
      
      if (response.data.token) {
        this.setAuthToken(response.data.token);
      }
      
      return response;
    } catch (error) {
      throw this.handleError(error);
    }
  }

  async createUser(userData) {
    try {
      return await this.client.post('/usuarios', userData);
    } catch (error) {
      throw this.handleError(error);
    }
  }

  async getUserById(id) {
    try {
      return await this.client.get(`/usuarios/${id}`);
    } catch (error) {
      throw this.handleError(error);
    }
  }

  async updateUser(id, userData) {
    try {
      return await this.client.patch(`/usuarios/${id}`, userData);
    } catch (error) {
      throw this.handleError(error);
    }
  }

  async deleteUser(id) {
    try {
      return await this.client.delete(`/usuarios/${id}`);
    } catch (error) {
      throw this.handleError(error);
    }
  }

  async getAllUsers() {
    try {
      return await this.client.get('/usuarios');
    } catch (error) {
      throw this.handleError(error);
    }
  }

  handleError(error) {
    if (error.response) {
      return {
        status: error.response.status,
        data: error.response.data,
        message: error.response.data?.message || error.message
      };
    }
    return {
      status: 500,
      message: error.message
    };
  }
}
module.exports = ApiClient;

