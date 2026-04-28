import axios from 'axios';

export abstract class BaseGeneratorService<
  TGenReq, TGenRes, TPushReq, TPushRes
> {
  protected abstract readonly basePath: string;

  async generate(request: TGenReq): Promise<TGenRes> {
    const response = await axios.post<TGenRes>(this.basePath, request);
    return response.data;
  }

  async downloadZip(configId: string): Promise<Blob> {
    const response = await axios.get(
      `${this.basePath}/${configId}/download`,
      { responseType: 'blob' },
    );
    return response.data as Blob;
  }

  async getFileContent(configId: string, filePath: string): Promise<string> {
    const response = await axios.get<{ content: string }>(
      `${this.basePath}/${configId}/files/${filePath}`,
    );
    return response.data.content;
  }

  async pushToGit(configId: string, request: TPushReq): Promise<TPushRes> {
    const response = await axios.post<TPushRes>(
      `${this.basePath}/${configId}/push-to-git`,
      request,
    );
    return response.data;
  }
}
