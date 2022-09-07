from flask import Flask, request
from tqdm import tqdm as q
from transformers import AutoTokenizer, AutoModelForMaskedLM
import re
import torch


def get_page_queries(page_text, context_size=49):
    res = []
    for i in q(range(1, len(page_text))):
        context = page_text[max(i-context_size, 0): i]
        label = page_text[i]
        # append a mask token to the context
        res.append((context + tokenizer.mask_token, label))
    return res


def importance_mask(predicted, query):
    # we define important characters to be the ones which are predicted incorrectly
    # given their context
    mask = []
    for pred, q in zip(predicted, query):
        target = q[1]
        if len(pred) > 0 and target in pred:
            mask.append(0)
        else:
            mask.append(1)
    return mask


def batchify(seq, size):
    # we have to batch the data to prevent OOM erros
    res = []
    while seq != []:
        res.append(seq[:size])
        seq = seq[size:]
    return res


def batch_predictor_cuda(texts, batch_size=10):
    res = []
    for batch in q(batchify(texts, batch_size)):
        # predict only one character ahead
        inputs = tokenizer(batch, return_tensors="pt", padding=True).to(device)
        token_logits = model(**inputs).logits
        mask_token_logits = token_logits[:, -2, :]
        top_10_tokens = torch.topk(mask_token_logits, 10, dim=1).indices.tolist()
        decoded = tokenizer.batch_decode(top_10_tokens, skip_special_tokens=True)
        res.extend(decoded)
    return res


def split_text_and_imask(text, imask):
    # it's like text.split(( |\n)), except we split the mask at the same locations too
    text_splits = []
    imask_splits = []

    split_locations = [x.start() for x in re.finditer('( |\n)', text)]
    split_locations += [len(text)]

    if len(split_locations) > 0:
        text_splits.append(text[:split_locations[0]])
        imask_splits.append(imask[:split_locations[0]])
        for i in range(1, len(split_locations)):
            text_splits.append(text[split_locations[i-1]+1:split_locations[i]])
            imask_splits.append(
                imask[split_locations[i-1]+1:split_locations[i]])
    else:
        text_splits = [text]

    return text_splits, imask_splits


def create_result_string(refined_imasks):
    res = ""
    for mask in refined_imasks:
        res = res + "".join([str(x) for x in mask]) + "0"
    return res[:-1]


def get_mask_from_text(text, refine=False, fill=False, context_size=49):
    queries = get_page_queries(text, context_size)
    query_texts = [q[0] for q in queries]
    predicted_results = batch_predictor_cuda(query_texts)
    imask = [1] + importance_mask(predicted_results, queries)

    text_splits, imask_splits = split_text_and_imask(text, imask)
    # not apply to Chinese
    # if refine:
    #     refined_imasks = refine_imasks(imask_splits, fill=fill)
    # else:
    #     refined_imasks = imask_splits
    return create_result_string(imask_splits)


if __name__ == '__main__':
    device = torch.device('cuda') if torch.cuda.is_available() else torch.device('cpu')
    tokenizer = AutoTokenizer.from_pretrained("hfl/chinese-macbert-base", padding_side='left')
    model = AutoModelForMaskedLM.from_pretrained("hfl/chinese-macbert-base").to(device)

    app = Flask(__name__)

    @app.route('/', methods=['POST', 'GET'])
    def handle_text_mask():
        text = request.values.get('text')
        should_refine = int(request.values.get('refine'))
        should_fill = int(request.values.get('fill'))
        context_size = int(request.values.get('context_size'))

        refined_imask = get_mask_from_text(
            f'{text}', should_refine, should_fill, context_size)
        print('-' * 80)
        print('len(text):', len(text))
        print('len(imask):', len(refined_imask))

        return refined_imask

    app.run(host="0.0.0.0")