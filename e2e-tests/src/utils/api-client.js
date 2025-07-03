const axios = require('axios');


class ApiClient {
  constructor(baseURL = process.env.API_GATEWAY_URL || 'https://localhost:443') {
    this.client = axios.create({
      baseURL,
      timeout: 30000,
      headers: {
        'Content-Type': 'application/json'
      },
      
      httpsAgent: new (require('https')).Agent({
        rejectUnauthorized: false  
      })
    });

    this.client.interceptors.request.use(
      (config) => {
        if (this.authToken && !config.headers.Authorization) {
          config.headers.Authorization = `Bearer ${this.authToken}`;
        }
        console.log(`🔄 ${config.method?.toUpperCase()} ${config.url}`);
        return config;
      },
      (error) => {
        console.error('Request error:', error.message);
        return Promise.reject(error);
      }
    );

    this.client.interceptors.response.use(
      (response) => {
        console.log(`${response.status} ${response.config.method?.toUpperCase()} ${response.config.url}`);
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

  generateTestUser() {
    const timestamp = Date.now();

    const nombres = ['Juan', 'Pedro', 'Luis', 'Carlos', 'Miguel', 'Jose', 'Ana', 'Maria'];
    const apellidos = ['Garcia', 'Lopez', 'Martinez', 'Perez', 'Rodriguez', 'Sanchez', 'Gomez'];
    

    const randomNombre = nombres[Math.floor(Math.random() * nombres.length)];
    const randomApellido = apellidos[Math.floor(Math.random() * apellidos.length)];
    
    return {
      firstName: randomNombre,
      lastName: randomApellido,
      email: `test${timestamp}@example.com`,
      password: `TestPass${timestamp.toString().slice(-4)}A`,
      roleId: 2 
    };
  }

  setAuthToken(token) {
    this.authToken = token;
    if (token) {
      this.client.defaults.headers.common['Authorization'] = `Bearer ${token}`;
    } 
    else {
      delete this.client.defaults.headers.common['Authorization'];
    }
  }

  async login(email, password) {
    try {
      const response = await this.client.post('/auth/login', {
        email,
        password
      });
      
      if (response.data) {
        this.setAuthToken(response.data.token);
      }
      
      return response;
    } catch (error) {
      throw this.handleError(error);
    }
  }

  async createUser(userData) {
    try {
      
      const createRequest = {
        firstName: userData.firstName,
        lastName: userData.lastName, 
        email: userData.email,
        password: userData.password,
        confirmPassword: userData.password, 
        role: userData.roleId === 1 ? 'Admin' : 'Cliente' 
      };
      return await this.client.post('/usuarios', createRequest, {
        headers: {
          'Content-Type': 'application/json' 
        }
      });
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
      const formData = new FormData();
      
      if (userData.firstName) formData.append('firstName', userData.firstName);
      if (userData.lastName) formData.append('lastName', userData.lastName);
      if (userData.email) formData.append('email', userData.email);
      if (userData.role) formData.append('role', userData.role);
      return await this.client.patch(`/usuarios/${id}`, formData, {
        headers: {
          'Content-Type': 'multipart/form-data' 
        }
      });
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
      const formData = new FormData();
      
      return await this.client.get('/usuarios', {
        data: formData,
        headers: {
          'Content-Type': 'multipart/form-data'
        }
      });
    } catch (error) {
      throw this.handleError(error);
    }
  }

  handleError(error) {
    if (error.response) {
      const errorData = {
        status: error.response.status,
        data: error.response.data,
        message: error.response.data?.error || error.response.data?.message || error.message
      };
      throw errorData;
    }
    
    const errorData = {
      status: 500,
      message: error.message,
      data: { error: error.message }
    };
    
    throw errorData;
  }
}

module.exports = ApiClient;

